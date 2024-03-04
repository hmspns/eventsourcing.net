namespace EventSourcing.Net.Samples.Persistence.Pages;

using System.Text;
using Abstractions.Contracts;
using Abstractions.Types;
using Data;
using Engine.Rebuild;
using Microsoft.AspNetCore.Components;

public partial class RebuildComponent
{
    private readonly StringBuilder _sbLog = new StringBuilder();

    [Inject]
    private IViewsRebuilder ViewsRebuilder { get; set; }
    
    [Inject]
    private ApplicationDbContext Context { get; set; }
    
    private async Task Rebuild()
    {
        int batchSize = 10;
        try
        {
            ViewsRebuilder.OnBatchRebuilt += ViewsRebuilderOnBatchRebuilt;
            WriteLog("Rebuild started");
            WriteLog("Dropping views db");
            await Context.Database.EnsureDeletedAsync();
            WriteLog("Views db dropped");
            WriteLog("Creating views db");
            await Context.Database.EnsureCreatedAsync();
            WriteLog("Views db created");
            WriteLog("Starting rebuild");
            await ViewsRebuilder.Rebuild(batchSize);
            WriteLog("Rebuild done");
        }
        finally
        {
            ViewsRebuilder.OnBatchRebuilt -= ViewsRebuilderOnBatchRebuilt;
        }
    }

    private void ViewsRebuilderOnBatchRebuilt(object sender, ViewsBatchRebuiltEventArgs args)
    {
        WriteLog($"Batch done. Processed {args.EventsProcessed} events");
    }

    private void WriteLog(string message)
    {
        _sbLog.AppendLine(message);
        StateHasChanged();
    }
}