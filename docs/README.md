# Bonsai Boilerplate Documentation for AI Agents

## Overview

This repository is a production-ready boilerplate for building cross-platform C# GUI applications. It relies on **Avalonia UI 11+** for the presentation layer and **.NET 10** for the runtime. The architecture follows a strict **Model-View-ViewModel (MVVM)** pattern supported by **Dependency Injection (DI)** and the **Microsoft Generic Host**.

Use this guide to understand how to extend, refactor, or test the application.

## 🏗 Project Architecture

### Directory Structure

* **`src/Bonsai.Core`**: The domain layer. Contains interfaces, entities, and plain C# objects (POCOs). Dependencies here should be minimal.
* **`src/Bonsai.Services`**: Implementation layer for business logic and external infrastructure (e.g., File I/O, Network).
* **`src/Bonsai.UI`**: The application entry point and presentation layer. Contains Views (`.axaml`), ViewModels, and the DI setup.
* **`tests/`**: Contains unit tests and headless UI tests.
* **`buid-scripts/` & `Makefile`**: Infrastructure for CI/CD, publishing, and bundling (AppImage, MSI, DMG).

### Dependency Injection (DI) Strategy

The application utilizes `Microsoft.Extensions.Hosting` allows for a web-like startup experience.

* **Entry Point**: `src/Bonsai.UI/Program.cs`.
* **Registration**: All Services and ViewModels are registered in the `.ConfigureServices(...)` lambda.
* **View-ViewModel Binding**: ViewModels are resolved via DI. The `HostContainer.ServiceProvider` static accessor serves as a bridge for XAML-based resolution if necessary, but **constructor injection** is the preferred method for ViewModels.

## 🧩 Implementing Features (Workflow)

When adding a new feature, follow this hierarchical flow to maintain separation of concerns:

1. **Define the Contract**:
    * Create an `interface` in `Bonsai.Core` (e.g., `IUserService.cs`).
2. **Implement the Logic**:
    * Implement the interface in `Bonsai.Services` (e.g., `UserService.cs`).
3. **Register Dependencies**:
    * Open `src/Bonsai.UI/Program.cs`.
    * Register the service: `services.AddSingleton<IUserService, UserService>();`.
4. **Create the ViewModel**:
    * Add a class in `src/Bonsai.UI/ViewModels/`.
    * Inherit from `ViewModelBase` or `ObservableObject`.
    * Use **constructor injection** to get `IUserService`.
    * Use `[ObservableProperty]` and `[RelayCommand]` attributes from `CommunityToolkit.Mvvm` to reduce boilerplate.
    * **Important**: Register the ViewModel in `Program.cs`: `services.AddTransient<MyViewModel>();`.
5. **Create the View**:
    * Create the `.axaml` and `.axaml.cs` files in `src/Bonsai.UI`.
    * Bind the `DataContext` to your resolved ViewModel.

## 🛠 Build & Test Automation

The repository uses a `Makefile` to abstract complex `dotnet` CLI commands. Always use these targets when verifying your work.

### Common Commands

| Command | Description | Context for AI |
| :--- | :--- | :--- |
| `make restore` | Restores NuGet packages | Run if `.csproj` files change. |
| `make build` | Builds in Release mode | Run to verify compilation. |
| `make run` | Runs the specific UI project | Use this to verify runtime behavior. |
| `make test` | Runs tests in Headless mode | **CRITICAL**: Run this after every code change. |

### Packaging

The project is configured for **Single File** and **Trimmed** deployment.

* **Linux**: `make package-linux` -> Produces AppImage.
* **Windows**: `make package-win` -> Produces MSI.
* **macOS**: `make package-mac` -> Produces DMG.

**⚠️ Trimming Warning**: If you introduce a library that relies heavily on reflection (e.g., Newtonsoft.Json), you may need to add it to `src/Bonsai.UI/TrimmerDescriptor.xml` to prevent the linker from stripping its code during Release builds.

## 🧪 Testing Guidelines

Tests are located in `tests/Bonsai.Tests`.

* **ViewModel Tests**: Instantiate ViewModels directly, injecting mock services.
* **UI Tests**: Use Avalonia's headless platform (configured in `test` target) to verify bindings and UI logic without a physical display.
