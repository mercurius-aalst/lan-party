pipeline {
    agent any

    environment {
        DOTNET_VERSION  = '9.0'
        IMAGE_NAME      = 'livingwooods/mercurius-frontend'
        DOCKERHUB_CREDS = credentials('dockerhub')
    }

    options {
        buildDiscarder(logRotator(numToKeepStr: '20'))
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
    }

    stages {
        // ── Version ──────────────────────────────────────────────────────────
        stage('Version') {
            steps {
                script {
                    def tag = sh(script: "git tag --points-at HEAD | head -1", returnStdout: true).trim()
                    if (tag) {
                        env.IMAGE_TAG = tag.replaceAll('^v', '')
                    } else {
                        def short = sh(script: "git rev-parse --short HEAD", returnStdout: true).trim()
                        env.IMAGE_TAG = "0.0.0-build.${env.BUILD_NUMBER}.${short}"
                    }
                    echo "Image tag: ${env.IMAGE_TAG}"
                }
            }
        }

        // ── Restore ──────────────────────────────────────────────────────────
        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }

        // ── Build ─────────────────────────────────────────────────────────────
        stage('Build') {
            steps {
                sh 'dotnet build --no-restore --configuration Release'
            }
        }

        // ── Tests ──────────────────────────────────────────────────────────────
        stage('Tests') {
            parallel {
                // Debug pass — includes MockBackend contract tests
                stage('Unit & Contract Tests (Debug)') {
                    steps {
                        sh '''
                            dotnet test \
                                --no-restore \
                                --configuration Debug \
                                -p:IncludeMockBackend=true \
                                --logger "trx;LogFilePrefix=debug" \
                                --results-directory TestResults/Debug
                        '''
                    }
                    post {
                        always {
                            junit allowEmptyResults: true, testResults: 'TestResults/Debug/**/*.trx'
                        }
                    }
                }

                // Release pass — MockBackend excluded, mirrors production boundary
                stage('Production Boundary Tests (Release)') {
                    steps {
                        sh '''
                            dotnet test \
                                --no-restore \
                                --configuration Release \
                                -p:IncludeMockBackend=false \
                                --logger "trx;LogFilePrefix=release" \
                                --results-directory TestResults/Release
                        '''
                    }
                    post {
                        always {
                            junit allowEmptyResults: true, testResults: 'TestResults/Release/**/*.trx'
                        }
                    }
                }
            }
        }

        // ── Docker build ──────────────────────────────────────────────────────
        stage('Docker Build') {
            steps {
                sh """
                    docker build \
                        --tag ${IMAGE_NAME}:${IMAGE_TAG} \
                        --tag ${IMAGE_NAME}:latest \
                        --file Dockerfile \
                        .
                """
            }
        }

        // ── Trivy scan ────────────────────────────────────────────────────────
        stage('Security Scan') {
            steps {
                sh """
                    if ! command -v trivy &>/dev/null; then
                        curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh \
                            | sh -s -- -b /usr/local/bin
                    fi

                    trivy image \
                        --severity CRITICAL,HIGH,MEDIUM \
                        --exit-code 1 \
                        --no-progress \
                        --format table \
                        ${IMAGE_NAME}:${IMAGE_TAG}
                """
            }
        }

        // ── Push to Docker Hub (main branch only) ─────────────────────────────
        stage('Push') {
            when {
                branch 'main'
            }
            steps {
                sh """
                    echo "${DOCKERHUB_CREDS_PSW}" | docker login -u "${DOCKERHUB_CREDS_USR}" --password-stdin
                    docker push ${IMAGE_NAME}:${IMAGE_TAG}
                    docker push ${IMAGE_NAME}:latest
                """
            }
        }

        // ── Deploy to Kubernetes (main branch only) ───────────────────────────
        stage('Deploy') {
            when {
                branch 'main'
            }
            steps {
                withCredentials([file(credentialsId: 'kubeconfig', variable: 'KUBECONFIG')]) {
                    sh """
                        kubectl set image deployment/frontend \
                            frontend=${IMAGE_NAME}:${IMAGE_TAG} \
                            --namespace lan

                        kubectl rollout status deployment/frontend \
                            --namespace lan \
                            --timeout=120s
                    """
                }
            }
        }
    }

    post {
        always {
            sh "docker rmi ${IMAGE_NAME}:${IMAGE_TAG} ${IMAGE_NAME}:latest || true"
            cleanWs()
        }
        success {
            echo "✅ Frontend pipeline succeeded — ${IMAGE_NAME}:${IMAGE_TAG}"
        }
        failure {
            echo "❌ Frontend pipeline failed — check the stage output above"
        }
    }
}
