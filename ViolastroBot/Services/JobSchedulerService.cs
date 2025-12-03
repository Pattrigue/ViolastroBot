using Discord.WebSocket;
using Quartz;
using Quartz.Impl;

namespace ViolastroBot.Services;

public sealed class JobSchedulerService(DiscordSocketClient client) : ServiceBase
{
    private IScheduler _scheduler;

    public override async Task InitializeAsync()
    {
        var factory = new StdSchedulerFactory();
        _scheduler = await factory.GetScheduler();
        _scheduler.Context.Add("DiscordClient", client);

        await _scheduler.Start();
    }

    public async Task ScheduleCronJob<T>(string cronSchedule)
        where T : IJob
    {
        var job = JobBuilder.Create<T>().Build();

        var trigger = TriggerBuilder
            .Create()
            .WithCronSchedule(cronSchedule, x => x.InTimeZone(TimeZoneInfo.Local))
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}
