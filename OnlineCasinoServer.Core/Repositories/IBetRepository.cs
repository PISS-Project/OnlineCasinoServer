using OnlineCasinoServer.Core.DTOs;
using System.Collections.Generic;

namespace OnlineCasinoServer.Core.Repositories
{
    public interface IBetRepository
    { 
        DiceBetDto Get(int id);
        IEnumerable<DiceBetDto> GetBets(int userId, int skip, int take, string orderby, string filter);
        DiceBetDto Create(DiceBetDto bet);
        void Delete(int id);
    }
}
