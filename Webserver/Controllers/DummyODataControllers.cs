using Microsoft.AspNetCore.Mvc;
using ODataWebserver.Models;
using ODataWebserver.Global;

namespace ODataWebserver.Webserver.Controllers
{
    [Route("api/[controller]")] [ApiController] public class EmployeeController : ODataControllerBase<Employee> { public EmployeeController(DummyContext context) : base(context) { } }
    [Route("api/[controller]")] [ApiController] public class HyperParametersController : ODataControllerBase<HyperParameter> { public HyperParametersController(DummyContext context) : base(context) { } }
    [Route("api/[controller]")][ApiController]public class JobsController : ODataControllerBase<Job> { public JobsController(DummyContext context) : base(context) { } }
    [Route("api/[controller]")][ApiController]public class JobResultsController : ODataControllerBase<JobResult> { public JobResultsController(DummyContext context) : base(context) { } }
    [Route("api/[controller]")][ApiController]public class ValuesToOverwriteController : ODataControllerBase<ValueOverride> { public ValuesToOverwriteController(DummyContext context) : base(context) { } }
}