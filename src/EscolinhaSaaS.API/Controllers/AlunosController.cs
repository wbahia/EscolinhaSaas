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