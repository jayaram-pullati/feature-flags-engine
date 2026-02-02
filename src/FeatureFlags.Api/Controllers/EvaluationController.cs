using Asp.Versioning;
using FeatureFlags.Api.Contracts;
using FeatureFlags.Core.Evaluation;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/evaluate")]
public sealed class EvaluationController(FeatureFlagEvaluator evaluator) : ControllerBase
{
  [HttpPost("{key}")]
  public ActionResult<EvaluateFeatureResponse> Evaluate(string key, EvaluateFeatureRequest request)
  {
    var ctx = new FeatureEvaluationContext(
        userId: request.UserId,
        groupIds: request.GroupIds,
        region: request.Region
    );

    var result = evaluator.Evaluate(key, ctx);

    return Ok(new EvaluateFeatureResponse(
        Enabled: result.Enabled,
        Source: result.Source.ToString()
    ));
  }
}
