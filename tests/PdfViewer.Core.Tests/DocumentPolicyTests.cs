using PdfViewer.Core;
using Xunit;
namespace PdfViewer.Tests;
public class DocumentPolicyTests
{
 [Theory]
 [InlineData(612,792,4)] [InlineData(100000,1,3)] [InlineData(1,100000,3)]
 public void RasterIsBoundedAndNonzero(double width,double height,double scale)
 {
  var size=DocumentPolicy.RasterSize(width,height,scale);
  Assert.InRange(size.Width,1u,2560u); Assert.InRange(size.Height,1u,2560u);
 }
 [Fact] public void InvalidDimensionsRejected() => Assert.Throws<ArgumentOutOfRangeException>(()=>DocumentPolicy.RasterSize(double.NaN,100,1));
 [Theory] [InlineData(-1,10,1)] [InlineData(100,10,10)] [InlineData(1,0,0)]
 public void NavigationStaysWithinDocument(int input,int total,int expected) => Assert.Equal(expected,DocumentPolicy.ClampPage(input,total));
 [Fact] public void NonfiniteZoomDoesNotReachRenderer() => Assert.Equal(1,DocumentPolicy.ClampZoom(double.NaN));
}

public class PageGeometryTests
{
 [Theory] [InlineData(595,900,868)] [InlineData(842,900,868)] [InlineData(595,20,100)]
 public void FitUsesViewportWithoutChangingAspect(double width,double viewport,double expected)
 { Assert.Equal(expected,DocumentPolicy.PageWidth(width,viewport,true,1)); }
 [Theory] [InlineData(595,.5,297.5)] [InlineData(842,2,1684)]
 public void ZoomUsesNativePageSize(double width,double zoom,double expected)
 { Assert.Equal(expected,DocumentPolicy.PageWidth(width,900,false,zoom)); }
 [Theory] [InlineData(0,100)] [InlineData(-1,100)] [InlineData(100,0)]
 public void InvalidPageGeometryIsRejected(double width,double height)
 { Assert.Throws<InvalidDataException>(() => new PdfPageSize(width,height).Validate()); }
 [Theory] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)]
 public void InvalidScaleCannotGenerateInvalidRaster(double scale)
 { var size=DocumentPolicy.RasterSize(595,842,scale); Assert.InRange(size.Width,1u,2560u); }
}

public class ZoomDirectionTests
{
 [Theory] [InlineData(4,1.2)] [InlineData(.1,.8)] [InlineData(3,1.2)] [InlineData(.25,.8)]
 public void FitOutsideManualBoundsDoesNotReverseZoom(double current,double factor)
 { Assert.Null(DocumentPolicy.ZoomStep(current,factor)); }
 [Fact] public void NormalZoomStepStillWorks() => Assert.Equal(1.2,DocumentPolicy.ZoomStep(1,1.2));
}
