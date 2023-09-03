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
        // Define the job and tie it to our MyJob class
        IJobDetail job = JobBuilder.Create<T>().Build();
                    
        // Trigger the job to run now, and then every 40 seconds
        ITrigger trigger = TriggerBuilder.Create()
            .WithCronSchedule(cronSchedule) // Cron expression for every 1 minute
            .StartNow()
            .Build();
                    
        // Schedule the job using the job and trigger 
        await _scheduler.ScheduleJob(job, trigger);
    }
}