using BankApi.DTO;
using System.Threading.Tasks;

namespace BankApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetSummaryAsync();
    }
}
