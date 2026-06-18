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

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            // Возвращаем список всех проектов из таблицы Projects
            return await _context.Projects.ToListAsync();
        }

        public async Task SaveProjectAsync(string projectName, string filePath)
        {
            var existingProject = await _context.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (existingProject == null)
            {
                _context.Projects.Add(new Project { Name = projectName, OpenApiFilePath = filePath });
            }
            else
            {
                existingProject.OpenApiFilePath = filePath;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            // Находим проект по ID
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                // Удаляем проект
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Project?> GetProjectWithDetailsAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.TestRuns)
                .ThenInclude(t => t.Vulnerabilities)
                .Include(p => p.TestRuns)
                .ThenInclude(t => t.SecurityHeadersResults)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }
    }
}