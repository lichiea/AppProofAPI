using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProofAPI.Data;
using ProofAPI.Models;

namespace ProofAPI.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated(); // создаст БД и таблицы
        }

        public async Task SaveTestRunAsync(Project project, TestRun testRun)
        {
            // Найти или создать проект
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.Name == project.Name);
            if (existingProject == null)
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                testRun.ProjectId = project.Id;
            }
            else
            {
                testRun.ProjectId = existingProject.Id;
            }

            _context.TestRuns.Add(testRun);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TestRun>> GetHistoryAsync(int projectId)
        {
            return await _context.TestRuns
                .Include(t => t.Vulnerabilities)
                .Include(t => t.SecurityHeadersResults)
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.StartedAt)
                .ToListAsync();
        }
    }
}