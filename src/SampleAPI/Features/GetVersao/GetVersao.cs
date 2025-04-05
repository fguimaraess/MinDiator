using Dapper;
using System.Data;

namespace SampleAPI.Features.GetVersao2;

public partial class GetVersao
{
    public record Query(string versao) : IRequest<Response>;
    public class Response
    {
        public Response()
        {
        }
        public Response(int versao, string descricao, bool ativo)
        {
            Versao = versao;
            Descricao = descricao;
            Ativo = ativo;
        }

        public int Versao { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
    };

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IDbConnection _connection;
        public Handler(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {

            var query = @"
                    select versao FROM public.versoes
                    WHERE descricao = @versao;
                ";

            string result = await _connection.QueryFirstOrDefaultAsync<string?>(query, new { request.versao });
            return new Response { Descricao = result } ?? throw new KeyNotFoundException("Versão não encontrada.");
        }
    }
}
