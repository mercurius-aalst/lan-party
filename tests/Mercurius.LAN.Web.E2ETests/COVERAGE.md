# E2E Coverage Ledger

Source-gap audit of the Playwright suite in this folder against the frontend
`src/Mercurius.LAN.Web` and the backend checkout the fixture starts. The audited baseline is the
main-line frontend and backend source, including the repairs listed under "Included repairs" below.

- 229 executable cases across 30 test classes: 224 `[Fact]` plus 2 `[Theory]` carrying 5
  `[InlineData]` (`AuthenticationFlowTests.CancellingSignInFromAProtectedRouteReturnsHomeAsAnonymousVisitor`
  x2, `MatchScoringFormatTests.DialogShowsTheConfiguredMatchFormat` x3).
- Three access levels (anonymous, member, admin) plus the ownership states (captain, invitee,
  non-member/stranger) are covered in the roles table below.
- Every class joins the `Playwright E2E` xUnit collection (`DisableParallelization`). `E2ETestBase`
  truncates the fixture-owned `mercurius_e2e_*` database before each test, refusing any database
  without that prefix, and `DisposeAsync` closes leftover browser contexts and API clients so tests
  never depend on execution order or on development data.

Paths: `F` = `src/Mercurius.LAN.Web`, `B` = backend `src`, `T` = this project. Component and
service claims use `file:line`; test references use the stable `Class.Method` name.

## Included repairs

| Repair | Source | Spec / capability | Tests |
| --- | --- | --- | --- |
| Server-rendered branded recovery pages for direct 403/404 responses, kept interactive after the response | `F/Components/Pages/Status/StatusCodePage.razor:1-28`, `F/Program.cs:67` | `responsive-accessible-states` (direct HTTP status responses) | `AuthenticationFlowTests.MemberHasNoOrganizerToolsAndIsForbiddenOnAdminRoute`, `MemberCannotOpenTheAdminOnlyUserProfileEvenThoughThePublicApiListsTheUsername`, `AuthenticatedMemberVisitingUnknownRouteSeesNotFoundPage`, `SponsorManagementPlaywrightTests.NonAdminCannotOpenSponsorManagement`, `PublicSiteTests.UnknownRouteShowsNotFoundPageForAnonymousVisitor` |
| Forbidden page uses the framework default layout instead of a duplicate layout provider | `F/Components/Routes.razor:5-17` (`NotAuthorized` renders `PageTitle` + `StatusPage` only; `AuthorizeRouteView.DefaultLayout` supplies the shell) | `responsive-accessible-states` (forbidden recovery page stays interactive) | `AuthenticationFlowTests.ForbiddenPageFromDirectRequestStaysInteractiveThroughInCircuitNavigation` |
| Same-route status-page retry reruns the initial data load | `F/Components/Shared/StatusPage.razor:52`, `F/Components/Pages/Users/Profile.razor:15`, `F/Components/Pages/Teams/ManageTeams.razor:19` | `api-unavailable-resilience` | `ProfileAndOnboardingTests.ProfileLoadFailureShowsUnavailableStateAndRecovers`, `TeamManagementPageTests.TeamManagementLoadFailureShowsUnavailableStateAndRecovers`, `TournamentDetailLifecyclePlaywrightTests.TournamentDetailLoadFailureShowsRetryAndRecoversInPlace` |
| Authentication failure does not retry the protected destination | `F/Components/Auth/RedirectToLogin.razor:7` | `account-access` | `AuthenticationFlowTests.CancellingSignInFromAProtectedRouteReturnsHomeAsAnonymousVisitor` (x2), `DeepLinkedProtectedRouteReturnsToTheRequestedPageAfterSignIn` |
| Invite-dialog actions stay reachable during the empty search state | `F/Components/Pages/Teams/InviteUserDialog.razor:47-75`, `.razor.css:132-145,200-210` | `user-owned-team-management` | `TeamInviteFlowTests.InviteDialogShowsEmptyResultMessageAndCanBeCanceled` |
| Global search stays usable with authenticated desktop navigation | `F/Components/Layout/NavMenu.razor.css:582-587` (`.brand-nav-search` `min-width: 10rem`) | `site-navigation` capability change `keep-authenticated-header-search-usable` | `PublicUserProfileTests.AdminOpensUserResultFromGlobalSearch` |
| Personal-group fanout on member removal and team deletion | `B/Modules/Teams/Mercurius.Modules.Teams/Application/Services/TeamEventPublishingDecorator.cs` | removal/deletion live-refresh behavior | `TeamRealtimeTests.RemovingMemberUpdatesCaptainAndRemovedMemberWithoutReload`, `DeletingTeamRefreshesConnectedMembersAndInviteesWithoutReload` |

### Contact form email transport (test-only sink)

The fixture starts a loopback `LocalSmtpSink` on an ephemeral port and points the app's
`ContactEmail` options at it (`T/Infrastructure/LocalSmtpSink.cs:13-30`,
`T/Infrastructure/PlaywrightE2EFixture.cs:61,106-112`). It uses the .NET standard library only and
needs no external credentials, packages, or production changes. Covered on the real UI:

- Success envelope and headers: envelope sender and recipients plus the `From`, `To`, `Reply-To`
  and subject headers (`PublicSiteTests.ContactFormSendsEmailWithReplyToAndClearsAfterSuccess`).
- In-flight state: the held 250 acceptance keeps `Sending...` disabled with the draft intact, and
  the form clears once the acceptance is released (same test).
- Discord fallback: a non-email contact value makes `Reply-To` fall back to the configured sender
  (`ContactFormUsesSenderAsReplyToForDiscordContact`).
- Transport failure and retry: the rejected delivery surfaces the Discord fallback message, the
  draft stays filled, the button re-enables, and the retry succeeds
  (`ContactFormSurfacesMailTransportFailureForValidMessage`).

## Executed evidence (final pair green)

| Artifact | Result |
| --- | --- |
| `T/TestResults/full-round10/full-round10.trx` | 229 total, 229 executed, 229 passed, 0 failed, 0 notExecuted, 30 classes; finished 02:34:57 (+02:00) |
| `T/TestResults/full-round11/full-round11.trx` | 229 total, 229 executed, 229 passed, 0 failed, 0 notExecuted, 30 classes; finished 02:40:56 (+02:00) |
| `T/TestResults/full-round8/full-round8.trx` | 229 total, 229 executed, 229 passed, 0 failed, 0 notExecuted, 30 classes; finished 23:59:21 UTC |
| `T/TestResults/full-round9/full-round9.trx` | 228 passed, 1 failed; historical failure, cause fixed by the test-only synchronization |
| `T/TestResults/full-round7/full-round7.trx` | 227 total, 227 passed, 0 failed; historical, valid prior coverage (pre-SMTP expansion) |
| `T/TestResults/full-round6/full-round6.trx` | 227 total, 227 passed, 0 failed; historical, valid prior coverage (pre-SMTP expansion) |
| `T/TestResults/full-round5/full-round5.trx` | 226 total, 226 passed, 0 failed; historical, pre-forbidden-circuit fix |
| `T/TestResults/full-round4/full-round4.trx` | 226 total, 226 passed, 0 failed; historical, pre-forbidden-circuit fix |
| `T/TestResults/probe3/probe3.trx`, `probe4/probe4.trx`, `probe5/probe5.trx` | 4 total, 4 passed, 0 failed each (12 cases) |
| `T/TestResults/contact-assertion-run1/contact-assertion-run1.trx` and `T/TestResults/contact-assertion-run2/contact-assertion-run2.trx` | 2 total, 2 passed, 0 failed each |
| `T/TestResults/solo-failcheck9/svenp_MEAN_MACHINE_2026-10-03_02_06_13_net10.0.trx` | 1 total, 1 failed; same cancellation case |
| `T/TestResults/fix-probes/final-classes/final-classes.trx` | 17 total, 17 passed, 0 failed (affected registration tests) |
| `T/TestResults/fix-probes/final-captain1/final-captain1.trx`, `final-captain2/final-captain2.trx`, `final-captain3/final-captain3.trx` | 1 total, 1 passed, 0 failed each (captain cancellation) |
| `T/TestResults/fix-probes/lane1/lane-probe1.trx`, `lane2/lane-probe2.trx`, `lane3/lane-probe3.trx` | 4 total, 4 passed, 0 failed each |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.ContractTests\TestResults\frontend-final-expanded\svenp_MEAN_MACHINE_2026-10-03_02_41_28_net10.0.trx` | 364 total, 364 passed, 0 failed, 0 skipped (final frontend contracts) |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-forbidden-final\svenp_MEAN_MACHINE_2026-10-03_02_41_54_net10.0.trx` | 1 total, 1 passed, 0 failed (forbidden-page circuit) |
| `D:\Github Repositories\lan-party\.tmp\playwright-e2e\tests\Mercurius.LAN.Web.E2ETests\TestResults\solo-smtp-final\svenp_MEAN_MACHINE_2026-10-03_02_42_14_net10.0.trx` | 1 total, 1 passed, 0 failed (SMTP success) |
| `PublicSiteTests` across three fresh fixtures | 33 of 33 passed |

The frozen source has 229 cases across 30 classes and is green on two consecutive full runs:
`full-round10` and `full-round11` each pass all 229 cases with no failures and no unexecuted cases.
`full-round9` is the historical failure: its one case,
`RegistrationTeamAdministrationPlaywrightTests.CaptainCanKeepThenCycleTeamRegistrationCancellation`,
saw the `#registration` panel still offering "Manage registration" and "Cancel registration" with 0
participants after a reload that raced the cancellation, because the success toast can appear before
the frontend refresh completes. The fix is test-only and synchronizes on the observable completed
panel state by awaiting the exact `#registration` "Register now" button before the reload and
asserting it again after the reload, in both the captain team-registration and individual
registration tests.

Independent `final_code_review` of both final runs passed with minor findings, and the publication
gate passed. The review is read-only and no rerun is claimed: it independently parsed 229 of 229
passed with 30 classes for each run and scanned all 242 traces per run. Each run holds the 6 expected
403/404 console errors, 0 browser page errors, and 0 Mud duplicate-provider errors; all 27 API
exceptions per run are the expected `42P01` database-fault records. Accepted `CustomAutocomplete`
JS-disconnected teardown records were 2 in `full-round10` and 1 in `full-round11`, and the five-flow
mutations were checked to complete (204/200) before the refresh with state-specific pre- and
post-reload assertions.

The 227-case runs postdate the forbidden-page circuit fix but predate the SMTP expansion, and the
226-case runs predate the forbidden-page circuit fix, so those four are historical prior coverage
rather than evidence for this revision. The contact-assertion runs repeat the two SMTP-focused cases,
and `PublicSiteTests` passes 33 of 33 across three fresh fixtures at the current source.

The cancellation fix synchronizes five flows across three files: individual registration
cancellation (`RegistrationPlaywrightTests.cs`), roster accept and roster decline
(`RegistrationTeamPlaywrightTests.cs`), and the captain cancellation cycle plus roster swap
(`RegistrationTeamAdministrationPlaywrightTests.cs`). `fix-probes/final-classes` passes all 17
affected registration tests, and the three `final-captain*` runs each pass the captain case 1/1.
These probes preceded the green `full-round10`/`full-round11` pair and are consistent with it.

Command-level evidence, including the exact commands and raw result artifacts for the solution and
solo runs, is recorded in [`VERIFICATION.md`](VERIFICATION.md).

Traces and final-page screenshots live under `T/TestResults/PlaywrightE2E/<run-id>`, and
`show-trace` is documented in `README.md`.

## Routes (14 `@page`)

| Route | Source | Primary tests |
| --- | --- | --- |
| `/` | `F/Components/Pages/Home.razor:1` | `PublicSiteTests.AnonymousHomePagePresentsEventFactsAndEntryPoints` (asserts the seeded-empty sponsor default, `Home.razor:93`), `HomeTicketCallToActionNavigatesToTicketSection`, `HomeFeaturedTournamentsMirrorThePublishedOrderAndOpenTheirDetailPage`, `HomePageShowsTheEmptyTournamentStateWhenNoTournamentExists`, `HomeTournamentLoadFailureShowsTheRetryStateAndRecoversAfterTheApiIsRestored`, `HomeSponsorFailureDegradesOnlyTheSponsorSection` |
| `/info` | `F/Components/Pages/Info.razor:1` | `PublicSiteTests.InfoPageRendersPackingTicketsMenuAndContactSections`, `InfoNavigationDropdownJumpsToRequestedSection`, `InfoMenuClosesWhenTheInteractionOverlayIsClicked`, `ContactFormRejectsBlankSubmissionWithoutSending`, `ContactFormRejectsMessageShorterThanTenCharacters`, `ContactFormSendsEmailWithReplyToAndClearsAfterSuccess`, `ContactFormUsesSenderAsReplyToForDiscordContact`, `ContactFormSurfacesMailTransportFailureForValidMessage` |
| `/sponsors` | `F/Components/Pages/Sponsors.razor:1` | `PublicSiteTests.SponsorsPageRendersSeededTiersInOrderWithTierSpecificDescriptions`, `SponsorsPageShowsTheEmptyStateWhenNoSponsorExists`, `SponsorsPageLoadFailureReportsFailureToastAndRecoversOnReload` |
| `/privacy-policy` | `F/Components/Pages/PrivacyPolicy.razor:1` | `PublicSiteTests.PrivacyPolicyPageDescribesCollectedDataAndRights`, `FooterQuickLinksNavigateToTheLegalPage` |
| `/Error` | `F/Components/Pages/Error.razor:1` | `PublicSiteTests.ErrorPageRendersSystemFailureState` |
| `/status/{StatusCode:int}` | `F/Components/Pages/Status/StatusCodePage.razor:1` | 403 and 404 render their dedicated copy and are asserted through the router rows below; the remaining codes fall to the generic error branch (see the re-execution note) |
| `/complete-profile` | `F/Components/Pages/Users/CompleteProfile.razor:1` | `ProfileAndOnboardingTests.*` |
| `/profile` | `F/Components/Pages/Users/Profile.razor:1` | `ProfileAndOnboardingTests.ProfileEditPersistsEveryFieldAndTheNewUsername`, `ProfileSaveRequiresAValidUsernameAndDoesNotPersistInvalidEdits`, `DeleteAccountStaysDisabledUntilTheExactUsernameIsTyped`, `AbandoningDeleteConfirmationLeavesTheAccountIntact`, `DeleteAccountAnonymizesTheProfileAndEndsTheSession`, `UnverifiedProfileOffersResendVerificationAndReportsGenericOutcome`, `ProfilePasswordResetReportsGenericOutcome`, `ProfileLoadFailureShowsUnavailableStateAndRecovers` |
| `/users/{Username}` | `F/Components/Pages/Users/PublicUserProfile.razor:1` | `PublicUserProfileTests.*`, `AuthenticationFlowTests.MemberCannotOpenTheAdminOnlyUserProfileEvenThoughThePublicApiListsTheUsername` |
| `/teams/manage` | `F/Components/Pages/Teams/ManageTeams.razor:1` | `TeamManagementPageTests.*`, `TeamMembershipTests.*`, `TeamInviteFlowTests.*`, `TeamDeleteTests.*`, `TeamRealtimeTests.*`, `TeamAuthorizationTests.*` |
| `/teams/{TeamName}` | `F/Components/Pages/Teams/PublicTeamProfile.razor:1` | `TeamPublicProfileTests.*` |
| `/tournaments` | `F/Components/Pages/Tournaments/TournamentsOverview.razor:1` | `TournamentOverviewPlaywrightTests.*`, `PublicSiteTests.TournamentsOverviewLoadFailureShowsTheErrorStateAndRecoversOnRetry`, `TournamentsOverviewSponsorFailureDegradesOnlyTheSponsorSection` |
| `/tournaments/{TournamentId:guid}` | `F/Components/Pages/Tournaments/TournamentDetail.razor:1` | `TournamentDetailLifecyclePlaywrightTests.*`, `Match*` tests, `Leaderboard*` tests, `Registration*` tests, `TournamentPartnerAndSwissPlaywrightTests.*` |
| `/admin/sponsors` | `F/Components/Pages/Admin/SponsorManagement.razor:1` | `SponsorManagementPlaywrightTests.*`, `AuthenticationFlowTests.AdminSeesOrganizerToolsAndCanOpenSponsorManagement` |

The detail route parameter is `{TournamentId:guid}`, not `{id:guid}`
(`F/Components/Pages/Tournaments/TournamentDetail.razor:1`). The create dialog offers only
`SingleElimination`, `DoubleElimination` and `Leaderboard`
(`F/Components/Pages/Tournaments/AddTournamentDialog.razor.cs:36-41`), so tests that need
`RoundRobin`/`Swiss` seed through the API.

### Router and SSR states (no `@page`)

| State | Source | Tests |
| --- | --- | --- |
| 403 forbidden | `F/Components/Routes.razor:5-17` (`AuthorizeRouteView.NotAuthorized`) | `AuthenticationFlowTests.MemberHasNoOrganizerToolsAndIsForbiddenOnAdminRoute`, `ForbiddenPageFromDirectRequestStaysInteractiveThroughInCircuitNavigation`, `MemberCannotOpenTheAdminOnlyUserProfileEvenThoughThePublicApiListsTheUsername`, `SponsorManagementPlaywrightTests.NonAdminCannotOpenSponsorManagement` |
| 404 not found | `F/Components/Routes.razor:22-27` (`Router.NotFound`) | `PublicSiteTests.UnknownRouteShowsNotFoundPageForAnonymousVisitor`, `AuthenticationFlowTests.AuthenticatedMemberVisitingUnknownRouteSeesNotFoundPage` |
| anonymous challenge | `F/Components/Routes.razor:12-15` (`RedirectToLogin`) to `/account/login` | `AuthenticationFlowTests.AnonymousVisitorIsChallengedByTheIdentityProviderForProtectedRoutes`, `CancellingSignInFromAProtectedRouteReturnsHomeAsAnonymousVisitor` (x2), `TeamManagementPageTests.AnonymousVisitorIsSentToSignInFromTeamManagement`, `RegistrationPlaywrightTests.AnonymousVisitorIsAskedToSignInOnOpenTournament` |
| HTTP re-execution | `F/Program.cs:67` `UseStatusCodePagesWithReExecute("/status/{0}")` | the two status rows above |

`StatusCodePage` switches on the status code: `403` renders the forbidden copy, `404` the not-found
copy, and the default branch (`_ =>`) renders the generic error copy
(`F/Components/Pages/Status/StatusCodePage.razor:16-28`). The default branch is reached by a direct
`/status/{code}` request for any code other than 403/404; ordinary server errors are handled earlier
by `UseExceptionHandler("/Error", ...)` (`F/Program.cs:62`) before the status-code re-execution
(`F/Program.cs:67`), so the router rows below exercise only the two dedicated branches. The 403 and
404 branches are asserted through the router rows. A direct-request case for a non-403/404 code is
deliberately not duplicated: the default branch renders the same generic error `StatusPage` copy as
`/Error` (`F/Components/Pages/Error.razor:8` uses `common.error` and `common.unexpectedError`, the
same keys as `StatusCodePage.razor:20,27`), and that behavior already has its own case in
`PublicSiteTests.ErrorPageRendersSystemFailureState`. The new behavior exercised here is the
dedicated 403/404 recovery page, not the default branch, and the default branch is not claimed as
tested.

Shell auth routes `/account/login`, `/account/register`, `/account/logout` and the logout callback
are mapped in `F/Program.cs:76-141` and exercised through `AuthenticationFlowTests` and
`PlaywrightE2EFixture.LoginAsync`. `AuthenticationFlowTests.DeepLinkedProtectedRouteReturnsToTheRequestedPageAfterSignIn`
covers the protected-return after sign-in.

## Roles and ownership

| Role / ownership | Covered behavior | Tests |
| --- | --- | --- |
| anonymous | protected routes challenge; public shell theme/language/mobile/search; public team and user profiles | `AuthenticationFlowTests.AnonymousVisitorIsChallengedByTheIdentityProviderForProtectedRoutes`, `PublicSiteTests.LanguageSelectorSwitchesCultureToDutchAndItSurvivesReload`, `ThemeToggleFlipsTheLayoutThemeAndItSurvivesReload`, `MobileNavigationOverlayOpensAndClosesFromButtonBackdropAndEscape`, `TeamPublicProfileTests.AnonymousVisitorSeesRosterWithCaptainBadge`, `PublicUserProfileTests.AnonymousVisitorIsChallengedBeforeReachingTheParticipantProfile`, `SponsorManagementPlaywrightTests.AnonymousVisitorIsChallengedWhenOpeningSponsorManagement`, `TeamAuthorizationTests.AnonymousCallerCannotReadTeamManagementData` |
| member | sign-in/out, onboarding, profile edit/delete, individual registration, match participation | `AuthenticationFlowTests.MemberSignsInThroughTheLoginUiAndSignsOutAgain`, `ProfileAndOnboardingTests.*`, `RegistrationPlaywrightTests.MemberCanRegisterForIndividualTournamentAndRegistrationPersists`, `MatchLifecycleTests.*` |
| admin | organizer tools, sponsor management, tournament lifecycle, admin registration/leaderboard/resolution, global-search user results | `AuthenticationFlowTests.AdminSeesOrganizerToolsAndCanOpenSponsorManagement`, `SponsorManagementPlaywrightTests.*`, `TournamentDetailLifecyclePlaywrightTests.*`, `MatchAdminActionTests.*`, `RegistrationPlaywrightTests.AdminCanRemoveIndividualRegistrationFromAdministrationPanel`, `RegistrationTeamAdministrationPlaywrightTests.AdminCanRemoveTeamRegistrationWithReason`, `LeaderboardPlaywrightTests.AdminRecordsGuestAttemptThroughDialogAndStandingsPersist`, `MatchBracketProgressionTests.AdminReversalClearsTheAdvancedDownstreamSlot`, `PublicUserProfileTests.AdminOpensParticipantProfileWithLinkedIdentitiesAndEmptyMatchHistory`, `AdminOpensUserResultFromGlobalSearch` |
| captain | create team, invite/remove/transfer/logo/delete, roster submit and roster-edit gates | `TeamManagementPageTests.Captain*`, `TeamMembershipTests.Captain*`, `TeamDeleteTests.Captain*`, `TeamInviteFlowTests.Captain*`, `RegistrationTeamPlaywrightTests.Captain*`, `RegistrationTeamAdministrationPlaywrightTests.Captain*`, `MatchLifecycleTests.TeamCaptainSeesCaptainActionsAndCanConfirmTheEnd` |
| invitee | accept/decline invite and roster selection from the page and notification bell; expired/canceled/re-sent invites | `TeamInviteFlowTests.Invitee*`, `TeamInviteFlowTests.InviteNotification*`, `RegistrationNotificationPlaywrightTests.*` |
| non-member / stranger | captain-only API mutations and other members' invites rejected | `TeamAuthorizationTests.MemberCannotUseCaptainMutationsThroughTheApi`, `MemberCannotCancelSomeoneElsesInviteThroughTheApi`, `StrangerCannotReplyToSomeoneElsesInvite`, `MemberCannotLeaveOnBehalfOfAnotherMember`, `MatchAdminActionTests.MatchMutationEndpointsRejectAnonymousNonParticipantAndNonAdminCallers` |

## Backend domain states

| State / contract | Source | Frontend surface | Tests |
| --- | --- | --- | --- |
| `TournamentStatus` (Scheduled, InProgress, Completed, Canceled) | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/TournamentStatus.cs` | status chips, filters, lifecycle buttons | `TournamentOverviewPlaywrightTests.AnonymousVisitorSeesTournamentCardWithStatusAndNoAdminControls`, `StatusFilterChipHidesNonMatchingStatusesAndExposesPressedState`, `NonScheduledTournamentCardClosesRegistration`, `TournamentDetailLifecyclePlaywrightTests.AdminCanCancelScheduledAndResetCanceledTournament`, `InProgressTournamentCannotBeDeletedAndFinishCompletesIt` |
| `MatchLifecycleState` (8 values) | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/MatchLifecycleState.cs` | match dialog actions and states | `MatchLifecycleTests.BothSidesConfirmingTheEndUnlocksScoreReportingInSeparateSessions`, `OpponentReportingADifferentScoreOpensTheCorrectionWindow`, `DisputedMatchAllowsExactlyOneCorrectionPerSideAndCompletesWhenTheyAgree`, `MatchDeadlineTests.DisputeEscalatesToAdminResolutionWhenTheCorrectionWindowExpires`, `MatchAdminActionTests.*` |
| `BracketType` (5 values) | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/BracketType.cs` | bracket rendering and create-dialog choices | `MatchBracketViewTests.StartedSingleEliminationRendersEveryBracketMatchWithItsParticipants`, `DoubleEliminationBracketExposesUpperLowerAndGrandFinalViews`, `RoundRobinTournamentRendersTheUnsupportedBracketNotice`, `LeaderboardTournamentReplacesTheBracketWithTheLeaderboard`, `TournamentAdminDialogPlaywrightTests.SelectingLeaderboardBracketSwapsParticipationForRankingMetric` |
| `TournamentFormat` (BestOf1/3/5) | `F/Models/Tournaments` and backend contracts | format label and score validation | `MatchScoringFormatTests.DialogShowsTheConfiguredMatchFormat`, `BestOfOneRejectsADrawReportAndKeepsTheMatchUnresolved`, `BestOfThreeRejectsAScoreBeyondTheDecisiveWin`, `BestOfFiveCompletesWhenBothSidesReportThreeToOne` |
| `TournamentRegistrationStatus` (PendingConfirmation, Active) and `TournamentRegistrationKind` (Individual, Team) | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/TournamentRegistrationStatus.cs`, `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/TournamentRegistrationKind.cs` | registration panels and admin removal | `RegistrationPlaywrightTests.MemberCanRegisterForIndividualTournamentAndRegistrationPersists`, `MemberCanCancelIndividualRegistrationThroughConfirmation`, `RegistrationIsClosedForNonScheduledTournament`, `RegistrationTeamPlaywrightTests.CaptainSubmitsRosterAndInvitedMemberAcceptingActivatesTheRegistration`, `CaptainCannotConfirmTeamRegistrationUntilTheRosterIsComplete` |
| `RosterMemberConfirmationStatus` (AutoConfirmed, Pending, Confirmed) | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/RosterMemberConfirmationStatus.cs` | roster chips and notification decisions | `RegistrationTeamPlaywrightTests.InvitedMemberDecliningRemovesThemFromTheRoster`, `RegistrationNotificationPlaywrightTests.InvitedMemberCanAcceptRosterSelectionFromNotificationBell`, `InvitedMemberCanDeclineRosterSelectionFromNotificationBell`, `RegistrationTeamAdministrationPlaywrightTests.CaptainSwapsAnActiveRosterMemberAndTheReplacementConfirmsToReactivate` |
| `TeamInviteStatus` (Pending, Accepted, Declined, Cancelled, Expired) | `B/Modules/Teams/Mercurius.Modules.Teams.Contracts/TeamInviteStatus.cs` | invite lists and notifications | `TeamInviteFlowTests.CaptainInvitesPlayerThenCancelsThePendingInvite`, `CaptainCannotReinviteAfterThreeDeclinesInsideCooldown`, `ExpiredPendingInviteIsHiddenFromTheInvitee`, `ExpiredPendingInviteIsSupersededWhenCaptainInvitesAgain`, `CanceledInviteCanBeSentAgain`, `CanceledInviteDisappearsForTheInviteeWithoutReload` |
| `LeaderboardParticipantKind` (LinkedUser, Guest) and the ranking metric | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/LeaderboardParticipantKind.cs` | guest badge, metric copy, ordering | `LeaderboardPlaywrightTests.AdminRecordsGuestAttemptThroughDialogAndStandingsPersist`, `AdminRecordsExistingLanUserAttemptWithoutGuestBadge`, `FastestTimeLeaderboardRanksShortestDurationFirst`, `LeaderboardValidationPlaywrightTests.GuestEntryRequiresDisplayNameAndResultValue` |
| `MatchResultKind` (Score, Forfeit, AdminResolution) | `B/Modules/Tournament/Mercurius.Modules.Tournament.Contracts/MatchResultKind.cs` | resolution, forfeit and reversal controls | `MatchLifecycleTests.ParticipantCanCancelAndThenConfirmAForfeit`, `MatchAdminActionTests.AdminResolvesADisputedMatchWithTheVerifiedScore`, `AdminReversalClearsTheOfficialResult`, `AdminForfeitConfirmationCanBeCancelledThenRecordsTheResult` |
| `SponsorTier` (Presenting, Gold, Silver, Bronze) | `B/Modules/Sponsorship/Mercurius.Modules.Sponsorship.Contracts/SponsorTier.cs` | tier sections and descriptions | `PublicSiteTests.SponsorsPageRendersSeededTiersInOrderWithTierSpecificDescriptions` |
| tournament finish gate on a ranked leaderboard result and on an unresolved final | backend service | disabled Finish plus reason | `LeaderboardPlaywrightTests.LeaderboardCannotBeFinishedUntilAResultExists`, `MatchBracketProgressionTests.FinishingATournamentWithAnUnresolvedFinalIsRefusedAndStaysOngoing`, `WinningTheFinalThenFinishingTheTournamentPublishesPlacements` (BestOf1 final won 1-0, then placements published) |
| realtime team invalidation | `F/Services/TeamRealtimeService.cs:70-71` | second-browser roster/team refresh without reload | `TeamRealtimeTests.CaptainRosterRefreshesWhenInviteeAcceptsInviteFromTheirOwnBrowser`, `CaptainRosterRefreshesWhenMemberLeavesWithoutReload`, `RemovingMemberUpdatesCaptainAndRemovedMemberWithoutReload`, `DeletingTeamRefreshesConnectedMembersAndInviteesWithoutReload` |

`TeamMembershipChangeKind` (Joined, Removed, Left) has no UI representation: the frontend only
receives `TeamMembershipChangedEvent` as an invalidation signal
(`F/DTOs/Participants/Teams/TeamManagementDTOs.cs:49`, `F/Services/TeamRealtimeService.cs:71`) and
refetches. `SearchIndexRebuildJobStatus` (Pending, Running, Completed, Failed) belongs to the
internal discovery jobs, which no client calls.

### Load-failure and error states

| Surface | Source | Tests |
| --- | --- | --- |
| `/profile` load failure | `F/Components/Pages/Users/Profile.razor:11` (shared `StatusPage`, `PrimaryForceLoad`) | `ProfileAndOnboardingTests.ProfileLoadFailureShowsUnavailableStateAndRecovers` |
| `/teams/manage` load failure | `F/Components/Pages/Teams/ManageTeams.razor:15` (shared `StatusPage`, `PrimaryForceLoad`) | `TeamManagementPageTests.TeamManagementLoadFailureShowsUnavailableStateAndRecovers` |
| `/teams/{TeamName}` load failure | `F/Components/Pages/Teams/PublicTeamProfile.razor:14` (no inline retry; recovers on reload) | `TeamPublicProfileTests.PublicTeamProfileLoadFailureShowsUnavailableStateAndRecovers` |
| `/tournaments/{id}` load failure | `F/Components/Pages/Tournaments/TournamentDetail.razor` in-place retry on the same route | `TournamentDetailLifecyclePlaywrightTests.TournamentDetailLoadFailureShowsRetryAndRecoversInPlace` |
| `/` tournament/sponsor failure | `F/Components/Pages/Home.razor:79-93` | `PublicSiteTests.HomeTournamentLoadFailureShowsTheRetryStateAndRecoversAfterTheApiIsRestored`, `HomeSponsorFailureDegradesOnlyTheSponsorSection` |
| `/tournaments` load/sponsor failure | `F/Components/Pages/Tournaments/TournamentsOverview.razor.cs` | `PublicSiteTests.TournamentsOverviewLoadFailureShowsTheErrorStateAndRecoversOnRetry`, `TournamentsOverviewSponsorFailureDegradesOnlyTheSponsorSection` |
| unknown username | `F/Components/Pages/Users/PublicUserProfile.razor.cs:77`, `.razor:13-31` | `PublicUserProfileTests.UnknownUsernameShowsTheUnavailableStateRatherThanNotFound` |
| unknown team | `F/Components/Pages/Teams/PublicTeamProfile.razor` | `TeamPublicProfileTests.UnknownTeamNameShowsNotFoundStatusPage` |

## Per-class inventory (229 cases)

| Class | Cases |
| --- | --- |
| `AuthenticationFlowTests` | 15 |
| `InfrastructureSmokeTests` | 1 |
| `LeaderboardPlaywrightTests` | 7 |
| `LeaderboardValidationPlaywrightTests` | 3 |
| `MatchAdminActionTests` | 8 |
| `MatchBracketProgressionTests` | 5 |
| `MatchBracketViewTests` | 7 |
| `MatchDeadlineTests` | 3 |
| `MatchLifecycleTests` | 11 |
| `MatchScheduleFilterTests` | 3 |
| `MatchScoringFormatTests` | 8 |
| `ProfileAndOnboardingTests` | 15 |
| `PublicSiteTests` | 33 |
| `PublicUserProfileTests` | 6 |
| `RegistrationNotificationPlaywrightTests` | 2 |
| `RegistrationPlaywrightTests` | 7 |
| `RegistrationTeamAdministrationPlaywrightTests` | 5 |
| `RegistrationTeamPlaywrightTests` | 5 |
| `SponsorManagementPlaywrightTests` | 7 |
| `TeamAuthorizationTests` | 6 |
| `TeamDeleteTests` | 4 |
| `TeamInviteFlowTests` | 16 |
| `TeamManagementPageTests` | 10 |
| `TeamMembershipTests` | 7 |
| `TeamPublicProfileTests` | 7 |
| `TeamRealtimeTests` | 4 |
| `TournamentAdminDialogPlaywrightTests` | 4 |
| `TournamentDetailLifecyclePlaywrightTests` | 9 |
| `TournamentOverviewPlaywrightTests` | 8 |
| `TournamentPartnerAndSwissPlaywrightTests` | 3 |

## Source-backed discrepancies

1. **Swiss is creatable only through the API and never startable from the UI.** The create dialog
   omits Swiss (`F/Components/Pages/Tournaments/AddTournamentDialog.razor.cs:36-41`), the detail
   page blocks Start with `unsupportedSwissBracket`
   (`F/Components/Pages/Tournaments/TournamentDetail.razor.cs:315-316`), and the backend factory has
   no Swiss case, so it falls to the `NotSupportedException` default
   (`B/Modules/Tournament/Mercurius.Modules.Tournament/Application/Services/MatchModeratorFactory.cs:22`).
   UI path asserted by `TournamentPartnerAndSwissPlaywrightTests.SwissTournamentCannotBeStartedAndShowsUnsupportedReason`.
2. **RoundRobin is backend-supported but UI-unsupported.** The factory resolves
   `RoundRobinMatchModerator`
   (`B/Modules/Tournament/Mercurius.Modules.Tournament/Application/Services/MatchModeratorFactory.cs:20`)
   and the API can create and start a
   RoundRobin tournament, but the matches tab renders `bracketUnsupported` for it and the create
   dialog cannot select it. Asserted by
   `MatchBracketViewTests.RoundRobinTournamentRendersTheUnsupportedBracketNotice`.
3. **`SponsorContext` has four values and one producer.** The only placement writer hardcodes
   `SponsorContext.TournamentPartner`
   (`F/Components/Pages/Tournaments/TournamentDetail.razor.cs:845`), and the admin UI offers a
   single sponsor select. The label, eyebrow and display-order switches for `CateringPartner`,
   `InfrastructurePartner` and `PrizePartner` (`F/Extensions/SponsorExtensions.cs:43-74`) are
   therefore unreachable.
4. **`/users/{Username}` is admin-only while search is offered to everyone.** The page carries
   `@attribute [Authorize(Roles = "admin")]` (`F/Components/Pages/Users/PublicUserProfile.razor:2`)
   while the anonymous public API resolves any username
   (`B/Modules/Identity/Mercurius.Modules.Identity/Endpoints/UserEndpoints.cs:27-36`), and global
   search disables and ignores user results for non-admins
   (`F/Components/Layout/NavMenu.razor.cs:446,458-459`). Asserted by
   `AuthenticationFlowTests.MemberSearchResultForAnotherUserIsDisabledInGlobalSearch` and
   `MemberCannotOpenTheAdminOnlyUserProfileEvenThoughThePublicApiListsTheUsername`.
5. **Unknown username renders "unavailable", not "not found".** Every API failure, including the
   404, sets `_hasError` (`F/Components/Pages/Users/PublicUserProfile.razor.cs:77`), so the
   `_hasError` branch (`F/Components/Pages/Users/PublicUserProfile.razor:13-22`) wins over the
   `_profile is null` branch (`F/Components/Pages/Users/PublicUserProfile.razor:23-31`),
   which is unreachable from the UI. Asserted by
   `PublicUserProfileTests.UnknownUsernameShowsTheUnavailableStateRatherThanNotFound`. The
   equivalent team-profile not-found state is reachable
   (`TeamPublicProfileTests.UnknownTeamNameShowsNotFoundStatusPage`).
6. **Overview search/status/participation filters are client-side over one page.** `PageSize = 24`
   and only the fetched page is filtered
   (`F/Components/Pages/Tournaments/TournamentsOverview.razor.cs:63,139`), so a match on page two is
   invisible to the filters. Pagination itself is covered by
   `TournamentOverviewPlaywrightTests.PaginationShowsNextPageWhenMoreThanOnePageOfTournamentsExists`.
7. **Some destructive actions skip confirmation.** Sponsor delete and tournament
   cancel/reset/finish/delete run straight from `@onclick`
   (`F/Components/Pages/Admin/SponsorManagement.razor:76` and `.razor.cs:170`;
   `F/Components/Pages/Tournaments/TournamentDetail.razor:109,118,128,134` and
   `.razor.cs:372,390,399,408`), while team removal, leave and delete do confirm
   (`TeamMembershipTests.CaptainCanCancelTeammateRemoval`, `MemberCanCancelLeavingTeam`,
   `TeamDeleteTests.CaptainCanCancelTeamDeletion`). The lifecycle actions themselves are exercised
   by `TournamentDetailLifecyclePlaywrightTests.AdminCanCancelScheduledAndResetCanceledTournament`
   and `AdminCanDeleteScheduledTournamentAndReturnsToOverview`.

## Not E2E-applicable

- **Auth0-hosted flows.** Social connections, MFA and email verification/reset delivery: the
  fixture swaps in a local OIDC stub (`T/Infrastructure/LocalOidcServer.cs`) and the profile tests
  assert the generic toast only
  (`ProfileAndOnboardingTests.UnverifiedProfileOffersResendVerificationAndReportsGenericOutcome`,
  `ProfilePasswordResetReportsGenericOutcome`). The application under test still uses the real OIDC
  cookie and JWT chain against that local provider.
- **External email delivery.** The contact form now runs a real SMTP conversation against the
  fixture's loopback sink, so the success path is covered; delivery to an external recipient and a
  real mail provider remain outside the fixture.
- **Admin user CRUD** (`B/Modules/Identity/Mercurius.Modules.Identity/Endpoints/UserEndpoints.cs:108-144`)
  and **discovery rebuild jobs**
  (`B/Modules/Discovery/Mercurius.Modules.Discovery/Endpoints/DiscoveryEndpoints.cs:42-61`): no
  route, component or client caller reaches them; only `IUserClient` wrapper methods exist.
- **`PUT /v1/lan/matches/{matchId}`** (`F/APIClients/ILANClient.cs:238-239`): wrapped by
  `TournamentService.UpdateMatchScoresAsync` (`F/Services/TournamentService.cs:155-159`) but no
  component calls it; score entry uses `PUT /v1/lan/matches/{matchId}/score` instead.

## Gap scan

A source-level re-audit covering every route, page action, role and domain enum found no untested
flow beyond the cases already listed. The contact-form SMTP success path, previously listed as not
E2E-applicable because no SMTP server was configured, is now covered by the test-only loopback sink
and is no longer a gap. The remaining genuine gaps are the external ones above: Auth0-hosted
MFA/social flows, delivery to external mail recipients, and the backend-only surfaces with no UI
caller.

## Remaining product findings

Recorded as found, not fixed. These are lifecycle findings, not clean-log claims.

1. **`ManageTeams` prerender/HTTP-scope disposal race (non-fatal, intermittent).** In the
   `full-round6` fixture
   (`TestResults/PlaywrightE2E/20261002T231452_144388_cc8f6b4a18104f01bb7deb1643a5ce01`),
   `frontend.log` records at `23:18:54.328` UTC (lines 31959-31985,
   `fail: Microsoft.AspNetCore.Server.Kestrel[13]`) a `System.ObjectDisposedException` for
   `IServiceProvider`, with the stack
   `ManageTeams.RequestRenderAsync` -> `RenderIfActiveAsync` -> `LoadSummaryAsync` -> `InitializeAsync`
   -> `DisposeAsync`. Source anchors: `F/Components/Pages/Teams/ManageTeams.razor.cs:64,135,182,184-195,621-626`.
   The race surfaces during the first render for
   `TeamAuthorizationTests.MemberOnlySeesRosterAndMembershipControls` on `/teams/manage` (trace
   `163`). It recurs in the `full-round9` fixture
   (`TestResults/PlaywrightE2E/20261002T235953_80204_2ab1292db1c2479e9e9b718ce44ebd0f`) at
   `00:04:06.056` UTC (`frontend.log` lines 32083-32109, `fail: Kestrel[13]`) with the same stack,
   while the interactive page stayed healthy. The page's roster and control assertions passed, and
   the same log line is absent from the `full-round7` fixture
   (`TestResults/PlaywrightE2E/20261002T232042_180336_1613b11656d3497fbdcb373da090ec98`), so this is
   an intermittent SSR lifecycle race rather than browser teardown, and the frontend log is not
   clean. It did not recur in the final pair: both `full-round10` and `full-round11` frontend logs
   contain 0 `ObjectDisposedException` records.
2. **`CustomAutocomplete.DisposeListener` async-void teardown.** An explicit Sponsor-edit reload
   produces a `JSDisconnectedException` from `CustomAutocomplete<T>.DisposeListener`
   (`F/Components/Shared/CustomAutocomplete.razor.cs:95,98`). It appears in both fixtures
   (`frontend.log` `23:18:48.668`, lines 31069-31076, and `23:24:31.675`, lines 31109-31116;
   `fail: CircuitHost[111]`, trace `158`). The active page recovers, so it is a separate known
   teardown bug rather than a page regression.
3. **Mud provider active-circuit errors: 0** in the `full-round6` and `full-round7` logs
   (independent reviewer confirmation).

## Archive and review status

Four archived OpenSpec changes cover this work: `2026-10-02-fix-forbidden-page-circuit`,
`2026-10-02-fix-server-status-and-aria-states`,
`2026-10-02-keep-authenticated-header-search-usable`, and the SMTP expansion, which modifies an
existing capability rather than adding one. Strict validation passes with 32 checks on the frontend
and 31 on the backend, so the frontend count stays 32 rather than 33. The independent SMTP source
review passed with no findings, and the receipt-bound, draft-preserved, retry, and hold-cleanup items
were resolved against the real two-case TRXs. The runtime fatal-log review for `full-round8` and
`full-round9` is closed: full9 carried the `ManageTeams` disposal recurrence and the
`CustomAutocomplete` teardown record noted above. The source is frozen at 229 cases across 30
classes and the five-flow synchronization review passed. The final frontend contracts pass 364/364,
both final solo checks pass 1/1, the tester finished with 0 hosts left and PostgreSQL untouched, and
the backend is unchanged since its 705-case run. The independent final review and the publication
gate both passed, so the ledger is complete.
