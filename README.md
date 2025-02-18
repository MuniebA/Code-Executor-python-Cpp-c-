# Docker-Based Code Execution System

A secure, containerized code execution environment supporting multiple programming languages (Python, C, and C++). Built using ASP.NET Core, Docker, and Blazor WebAssembly.

## Features

- 🔒 Secure code execution in isolated Docker containers
- 🌐 Web-based code editor interface
- 🔄 Support for multiple programming languages (Python, C, C++)
- ⚡ Real-time code execution and output
- 🛡️ Resource usage limitations and security constraints
- 📝 Input/output capability for interactive programs

## Architecture

### Components

1. **Frontend (Blazor WebAssembly)**
   - Language selection
   - Code editor with syntax highlighting
   - Input/output display
   - Execution controls

2. **Backend (ASP.NET Core API)**
   - RESTful endpoints
   - Docker service integration
   - Request validation
   - Resource management

3. **Execution Environment (Docker)**
   - Isolated containers
   - Resource limitations
   - Security constraints
   - Multi-language support

## Security Features

- Memory limit: 50MB per container
- CPU quota: 50% limit
- Network access: Disabled
- Execution timeout: 10 seconds
- File system isolation
- Input sanitization
- Process restrictions

## Prerequisites

- .NET 7.0 SDK
- Docker Desktop
- Visual Studio 2022 or later
- Git

## Setup Instructions

1. Clone the repository
   ```bash
   git clone [repository-url]
   ```

2. Navigate to the project directory
   ```bash
   cd [project-directory]
   ```

3. Start Docker Desktop

4. Build and run the API project
   ```bash
   cd PythonExecutor.Api
   dotnet run
   ```

5. Build and run the Web project
   ```bash
   cd ../PythonExecutor.Web
   dotnet run
   ```

## Usage

1. Open a web browser and navigate to `https://localhost:7130`
2. Select a programming language from the dropdown
3. Enter your code in the editor
4. (Optional) Provide input if your program requires it
5. Click "Execute" to run the code
6. View the output in the results panel

## Example Code

### Python
```python
name = input('Enter your name: ')
print(f'Hello {name}!')
```

### C
```c
#include <stdio.h>

int main() {
    char name[100];
    printf("Enter your name: ");
    scanf("%s", name);
    printf("Hello %s!\n", name);
    return 0;
}
```

### C++
```cpp
#include <iostream>
#include <string>

int main() {
    std::string name;
    std::cout << "Enter your name: ";
    std::cin >> name;
    std::cout << "Hello " << name << "!\n";
    return 0;
}
```

## Testing

Run the test suite:
```bash
cd PythonExecutor.Tests
dotnet test
```

The test suite includes:
- Security tests
- Performance tests
- Concurrent execution tests
- Language-specific tests

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Docker for containerization
- Microsoft for ASP.NET Core and Blazor
- The open-source community

## Contact

Munieb Abdelrahman - muniebawad@gmail.com
