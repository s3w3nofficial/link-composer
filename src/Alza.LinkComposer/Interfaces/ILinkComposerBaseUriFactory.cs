using System;

namespace Alza.LinkComposer.Interfaces
{
    public interface ILinkComposerBaseUriFactory
    {
        Uri GetBaseUri(string projectName);
    }
}
