using Gridify;
using Investment.Application.DTOs;
using Investment.Domain.Common;

namespace Investment.Application.Services;
public interface ILookupService
{
    Task<Result<Paging<LookupResponse>>> ObterAtivosAsync(GridifyQuery query);
}

