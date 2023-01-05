using System;

namespace Alza.LinkComposer.Interfaces
{
    public interface ILinkComposerBaseUriProvider
    {
        Uri GetBaseUri(string projectName);
    }
}
