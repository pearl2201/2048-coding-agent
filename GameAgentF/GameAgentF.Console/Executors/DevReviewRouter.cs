using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

// The class MUST be marked 'partial' for the source generator to work
internal sealed partial class DevReviewRouter : Executor
{
    public DevReviewRouter(string id, ExecutorOptions? options = null, bool declareCrossRunShareable = false) : base(id, options, declareCrossRunShareable)
    {
    }

    [MessageHandler]
    public ValueTask<string> RouteNextStepAsync(string leadDevFeedback, IWorkflowContext context)
    {
        // If the Lead Dev response contains 'APPROVED', output a branch signal
        if (leadDevFeedback.Contains("APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult("LOOP_EXIT");
        }

        // Otherwise, send it back to the loop
        return ValueTask.FromResult("LOOP_BACK");
    }
}



