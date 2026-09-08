using System.Collections.Generic;

public interface IWormSectionHealthPresentation
{
    void BindSections(IReadOnlyList<WormSection> sections);

    void Clear();
}
