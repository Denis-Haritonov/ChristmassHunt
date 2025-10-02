using Events;
using Events.API.ViewModels;
using Events.Models;
using Events.Specifications;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventsRepository _repository;
    
    public EventsController(IEventsRepository repo) => _repository = repo;

    // GET /api/events
    [HttpGet]
    public async Task<ActionResult<List<EventViewModel>>> List(
        CancellationToken cancellationToken = default)
    {
        var items = (await _repository.ListAsync(EventSpecs.FiftyNewestEventsSpecification, cancellationToken));
        return Ok(items);
    }
    
    // GET /api/events/{id}
    [HttpGet("{id:int}", Name = "Events_Get")]
    public async Task<ActionResult<Event>> Get(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity is null ? NotFound() : Ok(entity);
    }

    // POST /api/events
    [HttpPost(Name = "Events_Create")]
    public async Task<ActionResult<Event>> Create([FromBody] EventViewModel input, CancellationToken ct)
    {
        var ms = Validate(input);
        if (ms is not null) return ValidationProblem(ms);

        input.Id = 0; // ensure new
        input.CreatedUtc = DateTime.UtcNow;

        //TODO Map viemodel to input
        
        var created = await _repository.AddAsync(null, ct);
        return CreatedAtRoute("Events_Get", new { id = created.Id }, created);
    }

    // PUT /api/events/{id}  (full replace)
    [HttpPut("{id:int}", Name = "Events_Update")]
    public async Task<ActionResult<Event>> Replace(int id, [FromBody] EventViewModel input, CancellationToken ct)
    {
        if (id != input.Id) ModelState.AddModelError("id", "Route id must match body id.");
        var ms = Validate(input);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (ms is not null) return ValidationProblem(ms);

        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        //TODO

        await _repository.UpdateAsync(existing, ct);
        return Ok(existing);
    }
    

    // DELETE /api/events/{id}
    [HttpDelete("{id:int}", Name = "Events_Delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var e = await _repository.GetByIdAsync(id, ct);
        if (e is null) return NotFound();

        await _repository.DeleteAsync(e, ct);
        return NoContent();
    }

    // ---- validation (super small) ----
    private Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary? Validate(EventViewModel e)
    {
        var ms = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        if (string.IsNullOrWhiteSpace(e.Title)) ms.AddModelError("title", "Title is required.");
        if (e.EndsAtUtc < e.StartsAtUtc) ms.AddModelError("endsAtUtc", "EndsAtUtc must be ≥ StartsAtUtc.");
        return ms.ErrorCount > 0 ? ms : null;
    }
}



