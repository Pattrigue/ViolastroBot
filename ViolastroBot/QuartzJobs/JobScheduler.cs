using Discord.WebSocket;
using Quartz;
using Quartz.Impl;

namespace ViolastroBot.QuartzJobs;

public sealed class JobScheduler
{
    private IScheduler _scheduler;
    
    public async Task InitializeAsync(DiscordSocketClient client)
    {
        StdSchedulerFactory factory = new StdSchedulerFactory();
        _scheduler = await factory.GetScheduler();
        _scheduler.Context.Add("DiscordClient", client);
        
        await _scheduler.Start();
    }
    
    public async Task ScheduleCronJob<T>(string cronSchedule) where T : IJob
    {
        IJobDetail job = JobBuilder.Create<T>().Build();
                    
        ITrigger trigger = TriggerBuilder.Create()
            .WithCronSchedule(cronSchedule, x => x.InTimeZone(TimeZoneInfo.Local))
            .StartNow()
            .Build();
                    
        await _scheduler.ScheduleJob(job, trigger);
    }
}