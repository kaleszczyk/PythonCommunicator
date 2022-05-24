using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap source = new Bitmap(@"C:\Users\VI\Desktop\test.png"); 
            TrackSegmentationService trackSegmentationService = new TrackSegmentationService("infer_track_fix.py", "ISR_35epochs_1120x1024", @"C:\Docs\VI.AOD.Tests\VI.AOD.AutomaticObjectDetection\ExternalResources_pt1_GPU0\TrackSegmentation", 3440640, new Size(1120, 1024), 11000);
            trackSegmentationService.Initialize();
            Bitmap segmentationResult = trackSegmentationService.Evaluate(source); 
        }

        
    }
}
