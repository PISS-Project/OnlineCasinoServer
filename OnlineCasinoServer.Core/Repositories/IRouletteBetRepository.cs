using OnlineCasinoServer.Core.DTOs;
using System.Collections.Generic;

namespace OnlineCasinoServer.Core.Repositories
{
    public interface IRouletteBetRepository
    {
        RouletteBetDto Get(int id);
        RouletteBetDto Create(RouletteBetDto bet);
        IEnumerable<RouletteBetDto> GetBets(int userId, int skip, int take, string orderby, string filter);
    }
}
