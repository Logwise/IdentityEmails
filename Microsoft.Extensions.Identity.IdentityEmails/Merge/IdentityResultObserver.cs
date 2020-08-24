using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.Merge
{
    public class IdentityResultObserver
    {
        private List<IdentityResult> _results = new List<IdentityResult>();

        public bool HasErrors => _results.Any(r => r.Errors.Any());
        public bool ThrowOnError = true;

        public async Task Observe(Func<Task<IdentityResult>> work)
        {
            var res = await work();
            _results.Add(res);
            
            if (ThrowOnError && HasErrors)
            {
                Throw();
            }
        }

        public void Throw()
        {
            throw new Exception(
                string.Join(Environment.NewLine, _results.SelectMany(r => r.Errors.Select(x => $"{x.Code}: {x.Description}")))
            );
        }
    }
}