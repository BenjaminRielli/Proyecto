using NUnit.Framework;
using Library;

namespace Test;

public class CoordTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(10, 0)]
    [TestCase(0, 15)]
    [TestCase(25, 25)]
    public void CreaciónCorrectaDeCoords(int x, int y)
    {
        var coord = new Coord(x, y);

        Assert.AreEqual(x, coord.X);
        Assert.AreEqual(y, coord.Y);
    }

    [Test]
    [TestCase(-1, 0)]
    [TestCase(-1, 5)]
    [TestCase(0, -1)]
    [TestCase(5, -1)]
    [TestCase(-1, -1)]
    [TestCase(26, 0)]
    [TestCase(0, 26)]
    public void CreaciónIncorrectaDeCoords(int x, int y)
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            var coord = new Coord(x, y);
        });
    }

    [Test]
    [TestCase(0, 0, "A01")]
    [TestCase(0, 0, "a01")]
    [TestCase(0, 0, "A1")]
    [TestCase(0, 0, "A  01")]
    [TestCase(0, 0, "a  1")]
    [TestCase(0, 8, " a  09 ")]
    [TestCase(3, 14, "D15")]
    [TestCase(25, 14, "z15")]
    public void ConstructorConString(int x, int y, string coord)
    {
        var c = new Coord(coord);
        Assert.AreEqual(x, c.X);
        Assert.AreEqual(y, c.Y);
    }

    [Test]
    [TestCase("#01")]
    [TestCase("01")]
    [TestCase("01A")]
    [TestCase("A-01")]
    [TestCase("-30")]
    public void ConstructorFormatoIncorrectoConString(string coord)
    {
        Assert.Throws<System.ArgumentException>(() =>
        {
            var c = new Coord(coord);
        });
    }

    [Test]
    [TestCase("B27")]
    [TestCase("B0")]
    public void ConstructorRangoIncorrectoConString(string coord)
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() =>
        {
            var c = new Coord(coord);
        });
    }

    [Test]
    [TestCase(0, 0, "A01")]
    [TestCase(2, 4, "C05")]
    [TestCase(10, 11, "K12")]
    [TestCase(25, 11, "Z12")]
    public void Alfanuméricos(int x, int y, string alfanumérico)
    {
        var coord = new Coord(x, y);
        Assert.AreEqual(alfanumérico, coord.ToAlfanumérico());
    }

    [Test]
    [TestCase(0, 0, "A")]
    [TestCase(9, 0, "J")]
    [TestCase(25, 0, "Z")]
    public void ToStringXTest(int x, int y, string expected)
    {
        var coord = new Coord(x, y);
        Assert.AreEqual(expected, new Coord(x, y).ToStringX());
    }

    [Test]
    [TestCase(0, 0, "01")]
    [TestCase(0, 5, "06")]
    [TestCase(0, 25, "26")]
    public void ToStringYTest(int x, int y, string expected)
    {
        var coord = new Coord(x, y);
        Assert.AreEqual(expected, new Coord(x, y).ToStringY());
    }
}
