using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Objects;

public class BasicAbstractStick : AbstractPhysicalObject.AbstractObjectStick
{
    public AbstractPhysicalObject objectOne;
    public AbstractPhysicalObject objectTwo;

    public BasicAbstractStick(AbstractPhysicalObject objectOne, AbstractPhysicalObject objectTwo) : base(objectOne, objectTwo)
    {
        this.objectOne = objectOne;
        this.objectTwo = objectTwo;
    }
    public override string SaveToString(int roomIndex)
    {
        return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}", new object[]
        {
            roomIndex,
            objectOne.ID.ToString(),
            objectTwo.ID.ToString()
        }), "~", unrecognizedAttributes);
    }
}
