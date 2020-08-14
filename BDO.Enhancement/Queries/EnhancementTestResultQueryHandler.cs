using ZES.Infrastructure.Domain;
using ZES.Interfaces.Domain;

namespace BDO.Enhancement.Queries
{
    public class EnhancementTestResultQueryHandler : QueryHandlerBase<EnhancementTestResultQuery, EnhancementTestResult, EnhancementTestResultState>
    {
        public EnhancementTestResultQueryHandler(IProjectionManager manager) 
            : base(manager)
        {
        }

        protected override EnhancementTestResult Handle(IProjection<EnhancementTestResultState> projection, EnhancementTestResultQuery query)
        {
            if (!projection.State.NumberOfFailures.TryGetValue(query.EnhancementId, out var result))
                result = -1;
            
            return new EnhancementTestResult
            {
                NumberOfFailures = result,
            };
        }
    }
}