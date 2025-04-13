using GlucoPilot.Api.Endpoints.Injections.GetInjection;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Injections.ListInjections
{
    internal class ListInjectionsResponse
    {
        public int NumberOfPages { get; set; }
        public List<GetInjectionResponse> Injections { get; set; }
    }
}