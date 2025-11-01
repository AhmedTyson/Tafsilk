// STEP 1: Create Background Task Queue Interface
namespace TafsilkPlatform.Web.Services.BackgroundTasks;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

// STEP 2: Implement Background Task Queue
using System.Threading.Channels;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
 private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
          FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }

  public async ValueTask QueueBackgroundWorkItemAsync(
     Func<CancellationToken, ValueTask> workItem)
  {
        if (workItem == null)
        {
     throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}

// STEP 3: Create Background Worker Service
public class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceProvider serviceProvider,
      ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
 _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
       try
       {
  var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
       {
         await workItem(stoppingToken);
          }
       }
   catch (OperationCanceledException)
    {
             // Expected during shutdown
        }
    catch (Exception ex)
  {
       _logger.LogError(ex, "Error occurred executing background work item.");
         }
}

        _logger.LogInformation("Queued Hosted Service is stopping.");
    }
}

// STEP 4: Register in Program.cs
// builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
// builder.Services.AddHostedService<QueuedHostedService>();

// STEP 5: Update AccountController to Use Queue
// In ProvideTailorEvidence POST action:
/*
// REPLACE Task.Run with:
await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
{
    using var scope = _serviceProvider.CreateScope();
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountController>>();
    
    try
    {
        await emailService.SendEmailVerificationAsync(
     user.Email, 
        model.FullName, 
            verificationToken);
        logger.LogInformation("Email verification sent to tailor: {Email}", user.Email);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
    }
});
*/
