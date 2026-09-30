using Microsoft.EntityFrameworkCore;

namespace ToDo.Infrastructure.Database;

public class ToDoDbContext(DbContextOptions<ToDoDbContext> options) : DbContext(options);