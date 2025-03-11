using DevHabits.Api.Database;
using DevHabits.Api.DTOs.Habits;
using DevHabits.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Controllers;

[ApiController]
[Route("api/habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitDto>> GetHabits()
    {
        List<HabitDto> habits = await dbContext
            .Habits
            .Select(h => new HabitDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Type = h.Type,
                Frequency = new FrequencyDto
                {
                    Type = h.Frequency.Type,
                    TimesPerPeriod = h.Frequency.TimesPerPeriod
                },
                Target = new TargetDto
                {
                    Value = h.Target.Value,
                    Unit = h.Target.Unit
                },
                Status = h.Status,
                IsArchived = h.IsArchived,
                EndDate = h.EndDate,
                Milestone = h.Milestone == null ? null : new MilestoneDto
                {
                    Target = h.Milestone.Target,
                    Current = h.Milestone.Current,
                },
                CreatedAtUtc = h.CreatedAtUtc,
                UpdatedAtUtc = h.UpdatedAtUtc,
                LastCompletedAtUtc = h.LastCompletedAtUtc
            })
            .ToListAsync();

        return Ok(habits);

        //if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        //{
        //    return Problem(
        //        statusCode: StatusCodes.Status400BadRequest,
        //        detail: $"The provided sort parameter isn't valid: '{query.Sort}'");
        //}

        //if (!dataShapingService.Validate<HabitDto>(query.Fields))
        //{
        //    return Problem(
        //        statusCode: StatusCodes.Status400BadRequest,
        //        detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        //}

        //query.Search ??= query.Search?.Trim().ToLower();

        //SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        //IQueryable<HabitDto> habitsQuery = dbContext
        //    .Habits
        //    .Where(h => query.Search == null ||
        //                h.Name.ToLower().Contains(query.Search) ||
        //                h.Description != null && h.Description.ToLower().Contains(query.Search))
        //    .Where(h => query.Type == null || h.Type == query.Type)
        //    .Where(h => query.Status == null || h.Status == query.Status)
        //    .ApplySort(query.Sort, sortMappings)
        //    .Select(HabitQueries.ProjectToDto());

        //int totalCount = await habitsQuery.CountAsync();

        //List<HabitDto> habits = await habitsQuery
        //    .Skip((query.Page - 1) * query.PageSize)
        //    .Take(query.PageSize)
        //    .ToListAsync();

        //var paginationResult = new PaginationResult<ExpandoObject>
        //{
        //    Items = dataShapingService.ShapeCollectionData(habits, query.Fields),
        //    Page = query.Page,
        //    PageSize = query.PageSize,
        //    TotalCount = totalCount
        //};
    }

    //[HttpGet("{id}")]
    //public async Task<IActionResult> GetHabit(
    //    string id,
    //    string? fields,
    //    DataShapingService dataShapingService)
    //{
    //    if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
    //    {
    //        return Problem(
    //            statusCode: StatusCodes.Status400BadRequest,
    //            detail: $"The provided data shaping fields aren't valid: '{fields}'");
    //    }

    //    HabitWithTagsDto? habit = await dbContext
    //        .Habits
    //        .Where(h => h.Id == id)
    //        .Select(HabitQueries.ProjectToDtoWithTags())
    //        .FirstOrDefaultAsync();

    //    if (habit is null)
    //    {
    //        return NotFound();
    //    }

    //    ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, fields);

    //    return Ok(shapedHabitDto);
    //}

    //[HttpPost]
    //public async Task<ActionResult<HabitDto>> CreateHabit(
    //    CreateHabitDto createHabitDto,
    //    IValidator<CreateHabitDto> validator)
    //{
    //    await validator.ValidateAndThrowAsync(createHabitDto);

    //    Habit habit = createHabitDto.ToEntity();

    //    dbContext.Habits.Add(habit);

    //    await dbContext.SaveChangesAsync();

    //    HabitDto habitDto = habit.ToDto();

    //    return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    //}

    //[HttpPut("{id}")]
    //public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    //{
    //    Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

    //    if (habit is null)
    //    {
    //        return NotFound();
    //    }

    //    habit.UpdateFromDto(updateHabitDto);

    //    await dbContext.SaveChangesAsync();

    //    return NoContent();
    //}

    //[HttpPatch("{id}")]
    //public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    //{
    //    Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

    //    if (habit is null)
    //    {
    //        return NotFound();
    //    }

    //    HabitDto habitDto = habit.ToDto();

    //    patchDocument.ApplyTo(habitDto, ModelState);

    //    if (!TryValidateModel(habitDto))
    //    {
    //        return ValidationProblem(ModelState);
    //    }

    //    habit.Name = habitDto.Name;
    //    habit.Description = habitDto.Description;
    //    habit.UpdatedAtUtc = DateTime.UtcNow;

    //    await dbContext.SaveChangesAsync();

    //    return NoContent();
    //}

    //[HttpDelete("{id}")]
    //public async Task<ActionResult> DeleteHabit(string id)
    //{
    //    Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

    //    if (habit is null)
    //    {
    //        return NotFound();
    //    }

    //    dbContext.Habits.Remove(habit);

    //    await dbContext.SaveChangesAsync();

    //    return NoContent();
    //}
}
