# Verification record

Command-level evidence for the E2E suite and the repository checks run alongside it. Each result
below comes from a recorded run, and the artifacts are referenced by absolute path. This file records
the commands and outcomes; it is not a re-execution.

The E2E and frontend commands ran from the frontend repository root and the backend commands from the
backend repository root. The `--results-directory` arguments below are absolute and point at the
isolated verification checkouts used for these runs.

## Frontend solution (final)

```powershell
dotnet build Mercurius.LAN.sln -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Exit 0 with 4 NU1900 warnings and 0 errors.

```powershell
dotnet test Mercurius.LAN.sln --no-build --no-restore --logger trx --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.ContractTests\TestResults\frontend-final-expanded" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **364 passed, 0 failed, 0 skipped**
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.ContractTests\TestResults\frontend-final-expanded\svenp_MEAN_MACHINE_2026-10-03_02_41_28_net10.0.trx`).

An earlier 364-case run under
`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.ContractTests\TestResults\frontend-final`
predates the expanded source and is superseded by the run above.

## Backend solution

```powershell
dotnet restore LAN.API.sln -m:1 -nr:false -p:UseSharedCompilation=false --nologo
dotnet build LAN.API.sln -m:1 -nr:false -p:UseSharedCompilation=false --nologo
dotnet test LAN.API.sln --no-build --no-restore --logger trx --results-directory "D:\Github Repositories\lan-party\.tmp\backend-playwright-e2e\tests\TestResults\backend-final" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Restore and build exited 0 with 0 warnings.
- Test result: **705 passed, 0 failed, 0 skipped across 8 projects**
  (`D:\Github Repositories\lan-party\.tmp\backend-playwright-e2e\tests\TestResults\backend-final`).
- The backend has not changed since this run and is unchanged in the final state, so it remains the
  backend proof.

## E2E project

```powershell
dotnet restore tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj -m:1 -nr:false -p:UseSharedCompilation=false --nologo
dotnet build tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Restore exited 0 with one NU1900 advisory (service index unreachable).
- Build exited 0 with 2 NU1900 warnings and 0 errors.

## E2E suite (final pair, green)

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --logger "trx;LogFileName=full-round10.trx" --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round10" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **229 passed, 0 failed, 0 skipped** across 30 classes
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round10\full-round10.trx`,
  finished 02:34:57 (+02:00)). Exit 0.
- Fixture artifacts are under
  `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\PlaywrightE2E\20261003T002927_162276_e07634dc96ed4233b8cccaedaa465674`
  (242 traces, 0 page errors, 0 `ObjectDisposedException`, 0 Mud duplicate-provider errors, 2 accepted
  `CustomAutocomplete` cleanup records).

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --logger "trx;LogFileName=full-round11.trx" --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round11" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **229 passed, 0 failed, 0 skipped** across 30 classes
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round11\full-round11.trx`,
  finished 02:40:56 (+02:00)). Exit 0.
- Fixture artifacts are under
  `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\PlaywrightE2E\20261003T003528_137568_253b25e926014f4192a680a0206401b7`.
  The independent trace review passed; see "Final review" below.

Earlier 229-case green run, before the synchronization repair (coverage count only):

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --logger "trx;LogFileName=full-round8.trx" --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round8" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **229 passed, 0 failed, 0 skipped** across 30 classes
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round8\full-round8.trx`,
  finished 23:59:21 UTC).

Historical failure, cause fixed by the test-only synchronization:

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --logger "trx;LogFileName=full-round9.trx" --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round9" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **228 passed, 1 failed, 0 skipped** across 30 classes
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round9\full-round9.trx`,
  `ResultSummary outcome="Failed"`).
- Failing case:
  `RegistrationTeamAdministrationPlaywrightTests.CaptainCanKeepThenCycleTeamRegistrationCancellation`.
  After a reload that raced the cancellation, the `#registration` panel still offered "Manage
  registration" and "Cancel registration" with 0 participants, so the expected "Register now" button
  was absent. The fix is test-only and synchronizes on the observable completed panel state by
  awaiting the exact `#registration` "Register now" button before the reload and asserting it again
  after the reload, in both the captain team-registration and individual registration tests.
- Fixture artifacts are under
  `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\PlaywrightE2E\20261002T235953_80204_2ab1292db1c2479e9e9b718ce44ebd0f`,
  with trace 146 and the frontend log.

Historical runs at earlier revisions:

| Artifact | Result |
| --- | --- |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round7\full-round7.trx` | 227 passed, 0 failed, 0 skipped; finished 23:26:01 UTC |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round6\full-round6.trx` | 227 passed, 0 failed, 0 skipped; finished 23:20:20 UTC |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round5\full-round5.trx` | 226 passed, 0 failed, 0 skipped |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\full-round4\full-round4.trx` | 226 passed, 0 failed, 0 skipped; exact invocation not recorded |

The 227-case runs postdate the forbidden-page circuit fix but predate the SMTP expansion, and the
226-case runs predate the forbidden-page circuit fix.

## Cancellation fix probes

The fix synchronizes five flows across three files: individual registration cancellation
(`RegistrationPlaywrightTests.cs`), roster accept and roster decline
(`RegistrationTeamPlaywrightTests.cs`), and the captain cancellation cycle plus roster swap
(`RegistrationTeamAdministrationPlaywrightTests.cs`). Probe invocations were not recorded.

| Artifact | Result |
| --- | --- |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\final-classes\final-classes.trx` | 17 passed, 0 failed (affected registration tests) |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\final-captain1\final-captain1.trx` | 1 passed, 0 failed (captain cancellation) |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\final-captain2\final-captain2.trx` | 1 passed, 0 failed |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\final-captain3\final-captain3.trx` | 1 passed, 0 failed |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\lane1\lane-probe1.trx` | 4 passed, 0 failed |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\lane2\lane-probe2.trx` | 4 passed, 0 failed |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\fix-probes\lane3\lane-probe3.trx` | 4 passed, 0 failed |

These probes preceded the green `full-round10`/`full-round11` pair and are consistent with it.

## SMTP coverage

| Artifact | Result |
| --- | --- |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\contact-assertion-run1\contact-assertion-run1.trx` | 2 passed, 0 failed (`ContactFormSurfacesMailTransportFailureForValidMessage`, `ContactFormSendsEmailWithReplyToAndClearsAfterSuccess`) |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\contact-assertion-run2\contact-assertion-run2.trx` | 2 passed, 0 failed, same two cases |
| `PublicSiteTests` across three fresh fixtures | 33 of 33 passed |

The contact-assertion runs were invoked without a recorded command line. The contact form talks to
the fixture's loopback SMTP sink, so no external server or credentials are involved. The independent
SMTP source review passed with no findings.

## Solo runs

Final forbidden-page circuit check:

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --filter "FullyQualifiedName=Mercurius.LAN.Web.E2ETests.AuthenticationFlowTests.ForbiddenPageFromDirectRequestStaysInteractiveThroughInCircuitNavigation" --logger trx --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-forbidden-final" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **1 passed, 0 failed**
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-forbidden-final\svenp_MEAN_MACHINE_2026-10-03_02_41_54_net10.0.trx`).

Final SMTP success check:

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --filter "FullyQualifiedName=Mercurius.LAN.Web.E2ETests.PublicSiteTests.ContactFormSendsEmailWithReplyToAndClearsAfterSuccess" --logger trx --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-smtp-final" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Result: **1 passed, 0 failed**
  (`D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-smtp-final\svenp_MEAN_MACHINE_2026-10-03_02_42_14_net10.0.trx`).

Earlier solo checks:

```powershell
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --filter "FullyQualifiedName=Mercurius.LAN.Web.E2ETests.PublicUserProfileTests.AdminOpensUserResultFromGlobalSearch" --logger trx --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-search" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
dotnet test tests/Mercurius.LAN.Web.E2ETests/Mercurius.LAN.Web.E2ETests.csproj --no-build --no-restore --filter "FullyQualifiedName=Mercurius.LAN.Web.E2ETests.RegistrationNotificationPlaywrightTests.InvitedMemberCanAcceptRosterSelectionFromNotificationBell" --logger trx --results-directory "D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-roster" -m:1 -nr:false -p:UseSharedCompilation=false --nologo
```

- Results: **1 passed, 0 failed** each (`solo-search`, `solo-roster`).

## Hosts and runtime

- The final tester finished with 0 hosts left running, and PostgreSQL was untouched.
- The backend is unchanged since the 705-case run, so that result still stands.

## Final review

- Independent `final_code_review` of `full-round10` and `full-round11` passed with minor findings, and
  the publication gate passed. The review is read-only and no rerun is claimed. It independently
  parsed 229 of 229 passed with 30 classes for each run and scanned all 242 traces per run: each run
  holds the 6 expected 403/404 console errors, 0 browser page errors, and 0 Mud duplicate-provider
  errors; all 27 API exceptions per run are the expected `42P01` database-fault records. Accepted
  `CustomAutocomplete` JS-disconnected teardown records were 2 in `full-round10` and 1 in
  `full-round11`. The `ManageTeams` SSR finding is absent from both final runs but is retained as a
  historical finding in `COVERAGE.md`.

## Tooling checks

| Check | Result | Invocation |
| --- | --- | --- |
| CI workflow lint (actionlint) | 0 findings | not recorded |
| Strict OpenSpec validation, frontend (post-archive) | 32 passed, 0 failed | not recorded |
| Strict OpenSpec validation, backend (post-archive) | 31 passed, 0 failed | not recorded |

Actionlint is a static lint of the workflow file; the workflow runtime was not executed, and no
remote CI result is claimed.

## Notes

- Every `dotnet` command above ran with `-m:1 -nr:false -p:UseSharedCompilation=false` to keep the
  verification runs serial and deterministic.
- All restore and build commands exited 0.
