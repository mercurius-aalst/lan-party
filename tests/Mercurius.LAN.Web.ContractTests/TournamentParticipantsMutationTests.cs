using System.Reflection;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class TournamentParticipantsMutationTests
{
    private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [Fact]
    public async Task UserMutationStaleSuccessAfterTournamentChangeHasNoEffects()
    {
        var tournamentAId = Guid.NewGuid();
        var tournamentBId = Guid.NewGuid();
        var tournamentBRegistration = CreatePublicRegistration(Guid.NewGuid(), tournamentBId);
        var tournamentA = CreateTournament(tournamentAId);
        var tournamentB = CreateTournament(tournamentBId, tournamentBRegistration);
        var completion = NewCompletionSource<TournamentRegistrationDTO?>();
        var tab = CreateTab(out var toastService);
        var callbackCount = 0;
        SetUpdatedCallback(tab, _ => callbackCount++);

        tab.SetTournamentForTest(tournamentA);
        var mutation = InvokeUserMutation(tab, () => completion.Task);

        Assert.True(GetField<bool>(tab, "_isSubmitting"));
        tab.SetTournamentForTest(tournamentB);
        completion.SetResult(CreateRegistration(Guid.NewGuid(), tournamentAId));

        await mutation;

        Assert.Equal([tournamentBRegistration.Id], tournamentB.Registrations.Select(registration => registration.Id));
        Assert.Null(GetField(tab, "_registrationError"));
        Assert.False(GetField<bool>(tab, "_isSubmitting"));
        Assert.Empty(toastService.Messages);
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public async Task UserMutationStaleErrorAfterTournamentChangeHasNoEffects()
    {
        var tournamentA = CreateTournament(Guid.NewGuid());
        var tournamentB = CreateTournament(Guid.NewGuid());
        var completion = NewCompletionSource<TournamentRegistrationDTO?>();
        var tab = CreateTab(out var toastService);
        var callbackCount = 0;
        SetUpdatedCallback(tab, _ => callbackCount++);

        tab.SetTournamentForTest(tournamentA);
        var mutation = InvokeUserMutation(tab, () => completion.Task);

        tab.SetTournamentForTest(tournamentB);
        completion.SetException(new InvalidOperationException("Tournament A failed."));

        await mutation;

        Assert.Null(GetField(tab, "_registrationError"));
        Assert.False(GetField<bool>(tab, "_isSubmitting"));
        Assert.Empty(toastService.Messages);
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public async Task AdminMutationStaleSuccessAfterTournamentChangeHasNoEffects()
    {
        var tournamentAId = Guid.NewGuid();
        var tournamentBId = Guid.NewGuid();
        var tournamentBRegistration = CreatePublicRegistration(Guid.NewGuid(), tournamentBId);
        var tournamentA = CreateTournament(tournamentAId);
        var tournamentB = CreateTournament(tournamentBId, tournamentBRegistration);
        var completion = NewCompletionSource<bool>();
        var tab = CreateTab(out var toastService);
        var service = CreateTournamentService(completion);
        SetPrivateProperty(tab, "TournamentService", service);
        SetPrivateField(tab, "_adminRemovalReason", "Tournament A reason.");
        var callbackCount = 0;
        SetUpdatedCallback(tab, _ => callbackCount++);
        var registration = CreateAdminRegistration(Guid.NewGuid(), tournamentAId);

        tab.SetTournamentForTest(tournamentA);
        var mutation = InvokeAdminMutation(tab, registration);

        await service.CallStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal(registration.Id, GetField<Guid?>(tab, "_pendingAdminRemovalRegistrationId"));
        tab.SetTournamentForTest(tournamentB);
        SetPrivateField(tab, "_adminRemovalReason", "Tournament B reason.");
        completion.SetResult(true);

        await mutation;

        Assert.Equal([tournamentBRegistration.Id], tournamentB.Registrations.Select(item => item.Id));
        Assert.Equal("Tournament B reason.", GetField<string>(tab, "_adminRemovalReason"));
        Assert.Null(GetField(tab, "_adminError"));
        Assert.Null(GetField(tab, "_pendingAdminRemovalRegistrationId"));
        Assert.Empty(toastService.Messages);
        Assert.Equal(0, callbackCount);
        var call = Assert.Single(service.Calls);
        Assert.Equal(tournamentAId, call.TournamentId);
    }

    [Fact]
    public async Task AdminMutationStaleErrorAfterTournamentChangeHasNoEffects()
    {
        var tournamentA = CreateTournament(Guid.NewGuid());
        var tournamentB = CreateTournament(Guid.NewGuid());
        var completion = NewCompletionSource<bool>();
        var tab = CreateTab(out var toastService);
        var service = CreateTournamentService(completion);
        SetPrivateProperty(tab, "TournamentService", service);
        SetPrivateField(tab, "_adminRemovalReason", "Tournament A reason.");
        var callbackCount = 0;
        SetUpdatedCallback(tab, _ => callbackCount++);
        var registration = CreateAdminRegistration(Guid.NewGuid(), tournamentA.Id);

        tab.SetTournamentForTest(tournamentA);
        var mutation = InvokeAdminMutation(tab, registration);

        tab.SetTournamentForTest(tournamentB);
        SetPrivateField(tab, "_adminRemovalReason", "Tournament B reason.");
        completion.SetException(new InvalidOperationException("Tournament A failed."));

        await mutation;

        Assert.Equal("Tournament B reason.", GetField<string>(tab, "_adminRemovalReason"));
        Assert.Null(GetField(tab, "_adminError"));
        Assert.Null(GetField(tab, "_pendingAdminRemovalRegistrationId"));
        Assert.Empty(toastService.Messages);
        Assert.Equal(0, callbackCount);
    }

    private static TestableTournamentParticipantsTab CreateTab(out RecordingToastServiceProxy toastService)
    {
        var tab = new TestableTournamentParticipantsTab();
        var toast = DispatchProxy.Create<IToastService, RecordingToastServiceProxy>();
        toastService = (RecordingToastServiceProxy)(object)toast;
        SetPrivateProperty(tab, "ToastService", toast);
        return tab;
    }

    private static RecordingTournamentServiceProxy CreateTournamentService(TaskCompletionSource<bool> completion)
    {
        var service = DispatchProxy.Create<ITournamentService, RecordingTournamentServiceProxy>();
        var proxy = (RecordingTournamentServiceProxy)(object)service;
        proxy.RemovalCompletion = completion;
        return proxy;
    }

    private static Task InvokeUserMutation(
        TournamentParticipantsTab tab,
        Func<Task<TournamentRegistrationDTO?>> action)
    {
        var method = typeof(TournamentParticipantsTab).GetMethod(
            "RunRegistrationActionAsync",
            PrivateInstance,
            binder: null,
            [typeof(Func<Task>), typeof(string), typeof(Guid), typeof(long)],
            modifiers: null)!;
        var requestGeneration = GetField<long>(tab, "_requestGeneration") + 1;
        SetPrivateField(tab, "_requestGeneration", requestGeneration);
        var tournamentId = tab.Tournament.Id;
        Func<Task> mutation = async () => await action();
        return (Task)method.Invoke(tab, [mutation, "The registration changed.", tournamentId, requestGeneration])!;
    }

    private static Task InvokeAdminMutation(
        TournamentParticipantsTab tab,
        AdminTournamentRegistrationDTO registration)
    {
        var method = typeof(TournamentParticipantsTab).GetMethod(
            "RemoveAdminRegistrationAsync",
            PrivateInstance)!;
        var testTab = (TestableTournamentParticipantsTab)tab;
        // This test invokes the mutation directly; skip the component's initial
        // data load so the render triggered by the mutation cannot start a second
        // lifecycle operation with unconfigured test services.
        SetPrivateField(tab, "_hasLoadedForTournament", true);
        return testTab.InvokeOnRendererAsync(() => (Task)method.Invoke(tab, [registration])!);
    }

    private static TournamentExtended CreateTournament(
        Guid id,
        params PublicTournamentRegistrationDTO[] registrations) => new()
        {
            Id = id,
            Name = id.ToString(),
            Status = TournamentStatus.Scheduled,
            ParticipationMode = ParticipationMode.Individual,
            Registrations = registrations
        };

    private static PublicTournamentRegistrationDTO CreatePublicRegistration(Guid id, Guid tournamentId) => new()
    {
        Id = id,
        TournamentId = tournamentId,
        Kind = TournamentRegistrationKind.Individual,
        Status = TournamentRegistrationStatus.Active,
        User = CreateUser(Guid.NewGuid(), $"user-{id:N}")
    };

    private static TournamentRegistrationDTO CreateRegistration(Guid id, Guid tournamentId) => new()
    {
        Id = id,
        TournamentId = tournamentId,
        Kind = TournamentRegistrationKind.Individual,
        Status = TournamentRegistrationStatus.Active,
        User = CreateUser(Guid.NewGuid(), $"user-{id:N}")
    };

    private static AdminTournamentRegistrationDTO CreateAdminRegistration(Guid id, Guid tournamentId) => new()
    {
        Id = id,
        TournamentId = tournamentId,
        Kind = TournamentRegistrationKind.Individual,
        Status = TournamentRegistrationStatus.Active,
        User = CreateUser(Guid.NewGuid(), $"admin-user-{id:N}")
    };

    private static PublicUserDTO CreateUser(Guid id, string username) => new()
    {
        Id = id,
        Username = username,
        DisplayName = username
    };

    private static TaskCompletionSource<T> NewCompletionSource<T>() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void SetPrivateProperty(object instance, string name, object? value) =>
        typeof(TournamentParticipantsTab).GetProperty(name, PrivateInstance)!.SetValue(instance, value);

    private static void SetUpdatedCallback(
        TournamentParticipantsTab tab,
        Action<TournamentExtended> callback) =>
        typeof(TournamentParticipantsTab)
            .GetProperty(nameof(TournamentParticipantsTab.OnTournamentUpdated), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(tab, EventCallback.Factory.Create(tab, callback));

    private static void SetPrivateField(object instance, string name, object? value) =>
        typeof(TournamentParticipantsTab).GetField(name, PrivateInstance)!.SetValue(instance, value);

    private static T GetField<T>(object instance, string name) =>
        (T)typeof(TournamentParticipantsTab).GetField(name, PrivateInstance)!.GetValue(instance)!;

    private static object? GetField(object instance, string name) =>
        typeof(TournamentParticipantsTab).GetField(name, PrivateInstance)!.GetValue(instance);

    private sealed class TestableTournamentParticipantsTab : TournamentParticipantsTab
    {
        private readonly TestRenderer _renderer = new();

        public TestableTournamentParticipantsTab()
        {
            _renderer.Attach(this);
        }

        public void SetTournamentForTest(TournamentExtended tournament)
        {
            Tournament = tournament;
            OnParametersSet();
        }

        public Task InvokeOnRendererAsync(Func<Task> action) => _renderer.Dispatcher.InvokeAsync(action);
    }

    private sealed class TestRenderer : Renderer
    {
        public TestRenderer()
            : base(new ServiceCollection().BuildServiceProvider(), NullLoggerFactory.Instance)
        {
        }

        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

        public void Attach(IComponent component) => AssignRootComponentId(component);

        protected override void HandleException(Exception exception) => throw exception;

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }

    public class RecordingToastServiceProxy : DispatchProxy
    {
        public List<string> Messages { get; } = [];

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if(targetMethod is not null)
                Messages.Add(targetMethod.Name);

            return null;
        }
    }

    public class RecordingTournamentServiceProxy : DispatchProxy
    {
        public TaskCompletionSource<bool> RemovalCompletion { get; set; } = NewCompletionSource<bool>();
        public TaskCompletionSource<bool> CallStarted { get; } = NewCompletionSource<bool>();
        public List<(Guid TournamentId, Guid UserId, string? Reason)> Calls { get; } = [];

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if(targetMethod?.Name == nameof(ITournamentService.RemoveTournamentUserRegistrationAsAdminAsync))
            {
                Calls.Add(((Guid)args![0]!, (Guid)args[1]!, (string?)args[2]));
                CallStarted.TrySetResult(true);
                return RemovalCompletion.Task;
            }

            throw new NotSupportedException($"Unexpected tournament service call: {targetMethod?.Name}");
        }
    }
}
