using CTDT.Helper;
using Quartz.Impl;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.App_Start
{
    public class QuartzConfig
    {
        public static void ConfigureQuartz()
        {
            //ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            //IScheduler scheduler = schedulerFactory.GetScheduler().Result;
            //scheduler.Start().Wait();

            //IJobDetail job = JobBuilder.Create<DeleteOldClipboardAnswersJob>()
            //    .WithIdentity("deleteOldClipboardAnswersJob", "group1")
            //    .Build();

            //ITrigger trigger = TriggerBuilder.Create()
            //    .WithIdentity("trigger1", "group1")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithIntervalInMinutes(1)
            //        .RepeatForever())
            //    .Build();

            //scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}