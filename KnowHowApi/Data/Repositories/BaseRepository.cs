using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowHowApi.Data.Repositories
{
    public class BaseRepository
    {
    protected readonly Context _context;

    public BaseRepository(Context context)
    {
        _context = context;
    }
    }
}