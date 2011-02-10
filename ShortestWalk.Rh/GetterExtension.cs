using System;
using System.Collections.Generic;
using System.Text;

namespace ShortestWalk.Rh.Extensions
{
    static class GetterExtension
    {
        public static int AddEnumOptionList(CurvesGetter getLines, Enum enumerationCurrent)
        {
            Type type = enumerationCurrent.GetType();

            string[] names = Enum.GetNames(type);
            string current = Enum.GetName(type, enumerationCurrent);

            int location = Array.IndexOf(names, current);
            if (location == -1)
                throw new ArgumentException("enumerationCurrent is not an existing value");

            return getLines.AddOptionList(type.Name, names, location);
        }

        public static T RetrieveEnumOptionValue<T>(int resultIndex)
        {
            Type type = typeof(T);

            if (!type.IsEnum)
                throw new ApplicationException("T must be enum");

            Array values = Enum.GetValues(type);
            object current = values.GetValue(resultIndex);

            return (T) current;
        }
    }
}
