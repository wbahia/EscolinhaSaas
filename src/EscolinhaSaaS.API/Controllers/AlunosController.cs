using EscolinhaSaaS.Domain.Entities;
using EscolinhaSaaS.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolinhaSaaS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlunosController : ControllerBase
{
    private readonly TenantDbContext _context;

    public AlunosController(TenantDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Aluno aluno)
    {
        var schemaAtual = await _context.Database.ExecuteSqlRawAsync("SHOW search_path");
        _context.Alunos.Add(aluno);
        await _context.SaveChangesAsync();
        return Ok(aluno);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var alunos = await _context.Alunos.ToListAsync();
        return Ok(alunos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Aluno>> GetById(Guid id)
    {
        var aluno = await _context.Alunos.FindAsync(id);
        if (aluno == null) return NotFound();
        return aluno;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Aluno aluno)
    {
        if (id != aluno.Id) return BadRequest();

        // Garante que o TenantId não mude acidentalmente
        _context.Entry(aluno).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Alunos.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var aluno = await _context.Alunos.FindAsync(id);
        if (aluno == null) return NotFound();

        _context.Alunos.Remove(aluno);
        await _context.SaveChangesAsync();

        return NoContent();
    }

}