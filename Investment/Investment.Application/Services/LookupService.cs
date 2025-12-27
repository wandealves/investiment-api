using Gridify;
using Investment.Application.DTOs;
using Investment.Application.Mappers;
using Investment.Domain.Common;
using Investment.Infrastructure.Repositories;

namespace Investment.Application.Services;
public class LookupService(IAtivoRepository ativoRepository) : ILookupService
{
    public async Task<Result<Paging<LookupResponse>>> ObterAtivosAsync(GridifyQuery query)
    {
        var paging = await ativoRepository.ObterAsync(query);

        var responsePaging = new Paging<LookupResponse>
        {
            Count = paging.Count,
            Data = paging.Data.Select(AtivoMapper.ToLookupResponse).ToList()
        };

        return Result<Paging<LookupResponse>>.Success(responsePaging);
    }

    public Result<IList<LookupAnoResponse>> ObterAnosDisponiveis()
    {
        var anoAtual = DateTime.Now.Year;
        List<LookupAnoResponse> anos = new List<LookupAnoResponse> { new LookupAnoResponse(0) };
        for (var i = 0; i <= 20; i++)
        {
            anos.Add(new LookupAnoResponse(anoAtual - i));
        }
        return Result<IList<LookupAnoResponse>>.Success(anos);
    }
}

