using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ProofAPI.Models;

namespace AppProofAPI.ViewModels;

public class MenuItem
{
    public string Title { get; set; } = "";
    public string Icon { get; set; } = "";
    public string PageKey { get; set; } = "";
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private MenuItem? _selectedMenuItem;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private Project? _selectedProject;

    // Событие для запроса подтверждения удаления (MVVM паттерн)
    public event Func<Project, Task<bool>>? AskDeleteConfirmation;

    public ObservableCollection<MenuItem> MenuItems { get; } = new();
    private readonly System.Collections.Generic.Dictionary<string, object> _pages;
    private readonly ProofAPI.Services.DatabaseService _dbService;

    public MainWindowViewModel()
    {
        // Сервисы
        var authService = new ProofAPI.Services.AuthService();
        var importService = new ProofAPI.Services.DataImportService();
        var loadTestService = new ProofAPI.Services.LoadTestService(authService);
        var securityService = new ProofAPI.Services.SecurityTestService(authService);
        _dbService = new ProofAPI.Services.DatabaseService();
        var reportGen = new ProofAPI.Services.ReportGenerator();
        var testManager = new ProofAPI.Services.TestManager();
        var orchestrator = new ProofAPI.Services.TestOrchestrator(loadTestService, securityService, testManager, _dbService, reportGen);

        // Страницы
        var importVm = new ImportViewModel(importService, _dbService);
        importVm.ProjectSavedSuccessfully += () => _ = LoadProjectsAsync();
        var reportVm = new ReportViewModel(reportGen, _dbService);
        var testExecutionVm = new TestExecutionViewModel(orchestrator, _dbService, importService);

        _pages = new System.Collections.Generic.Dictionary<string, object>
        {
            [ "Import" ] = importVm,
            [ "TestExecution" ] = testExecutionVm,
            [ "Reports" ] = reportVm
        };

        // Пункты меню
        MenuItems.Add(new MenuItem { Title = "Импорт спецификации", Icon = "M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8zM14 2v6h6", PageKey = "Import" });
        MenuItems.Add(new MenuItem { Title = "Тесты", Icon = "M4 4h16v16H4z M9 9h6v6H9z", PageKey = "TestExecution" });
        MenuItems.Add(new MenuItem { Title = "Отчёты", Icon = "M14 2H6C4.9 2 4 2.9 4 4v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm-1 7V3.5L18.5 9H13zM9 20l2-5-2-4h6l-2 4 2 5H9z", PageKey = "Reports" });

        SelectedMenuItem = MenuItems.FirstOrDefault();

        // Загружаем проекты при старте
        _ = LoadProjectsAsync();
    }

    partial void OnSelectedMenuItemChanged(MenuItem? value)
    {
        if (value != null && _pages.TryGetValue(value.PageKey, out var page))
        {
            CurrentPage = page;
        }
    }

    partial void OnSelectedProjectChanged(Project? value)
    {
        if (value != null)
        {
            // Создаем ViewModel для детального просмотра проекта
            var detailsVm = new ProjectDetailsViewModel(_dbService, value);
            // Подменяем текущую страницу в рабочей области
            CurrentPage = detailsVm;
            SelectedMenuItem = null;
        }
    }

    [RelayCommand]
    private async Task DeleteProject(Project? project)
    {
        if (project == null) return;

        // Запрашиваем подтверждение через View (MainWindow.axaml.cs)
        if (AskDeleteConfirmation != null)
        {
            bool isConfirmed = await AskDeleteConfirmation.Invoke(project);
            if (!isConfirmed) return;
        }

        try
        {
            await _dbService.DeleteProjectAsync(project.Id);
            Projects.Remove(project);
            if (SelectedProject == project)
            {
                SelectedProject = Projects.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка удаления: {ex.Message}");
        }
    }

    // --- МЕТОД ЗАГРУЗКИ ПРОЕКТОВ ИЗ БД ---
    private async Task LoadProjectsAsync()
    {
        try
        {
            var projectsFromDb = await _dbService.GetAllProjectsAsync();
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Projects = new ObservableCollection<Project>(projectsFromDb);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки проектов: {ex.Message}");
        }
    }
}