using LibrarySystem.API.DataContext;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class FineTypeRepository : IFineTypeRepository
    {
        private readonly AppDbContext _context;

        public FineTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FineType> AddFineTypeAsync(FineType fineType)
        {
            await _context.FineTypes.AddAsync(fineType);
            await _context.SaveChangesAsync();
            return fineType;
        }

        public async Task<FineType> UpdateFineTypeAsync(FineType fineType)
        {
            _context.FineTypes.Update(fineType);
            await _context.SaveChangesAsync();
            return fineType;
        }

        public async Task<FineType?> GetByIdAsync(int id)
        {
            return await _context.FineTypes.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<FineType>> GetAllFineTypesAsync()
        {
            return await _context.FineTypes.ToListAsync();
        }

        public async Task<FineType?> GetByNameAsync(string name)
        {
            return await _context.FineTypes
                    .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

        }
    }
}
