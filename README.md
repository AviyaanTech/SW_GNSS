# SW_GNSS
SW GNSS is a free software for post-processing GNSS data. It supports static, kinematic and PPP processing.

Corrected positions can be exported as Excel files. The correction can be applied to an SW Maps project, and the corrected project can be exported as Shapefiles, GPKG or KMZ.

RINEX, u-blox UBX, Septentrio SBF and RTCM3 files are supported as input for rover and base station. 

The post-processing engine is based on the open source RTKLIB program by T. Takasu.

http://www.rtklib.com/

SW GNSS includes the modified version of RTKLIB by rtklibexplorer. RTKLIB is included as executable files and users may upgrade or change the executables as required.

https://github.com/rtklibexplorer/RTKLIB

The application can automatically download PPP correction files from IGS and the European Space Agency.

http://navigation-office.esa.int/products/gnss-products/

# Usage Instructions

## Adding Rover and Base Files
You can add rover and base station files by pressing the Add button in the respective input tables on the left side. 

## Processing Options

### Processing Mode
One of the processing modes among Kinematic, Static, Single, PPP-Static and PPP-Kinematic must be selected. 
1. Kinematic: To calculate corrected positions for a moving rover using a static base station. Position is output as a time series.
2. Static: To calculate corrected positions for a static rover using a static base station. Position may be output as a single point or as a time series based on settings.
3. Single: No corrections applied. Position is computed based on standalone observations of the rover.
4. PPP-Static: Similar to Static, but uses precise orbits and clocks instead of reference station data.
5. PPP-Kinematic: Similar to Kinematic, but uses precise orbits and clocks instead of reference station data.

### Configuration
Configuration can be changed by pressing the settings icon next to the processing mode dropdown.
1. Elevation Mask: The elevation mask is used to reject satellites below a given elevation angle.
2. SNR Mask: Uset to reject satellites having SNR below the given value. The rover and base files must contain SNR information if this is not set to zero.
3. Output Static Results as Single Point: If enabled, only one point (averaged coordinates) is output for static processing.
4. Satellites: Select which satellite systems to use among GPS, GLONASS (GLO), Galileo (GAL), BeiDou (BDS), QZSS, SBAS and IRNSS.
5. Rover Antenna/Base Antenna: The rover/base antenna model used to apply antenna calibration corrections. Select the correct antenna model from the list. List is generated from the ngs14.atx file.
6. Antenna Delta: Difference in position between the antenna reference point and the marker coordinates.
7. Base Position: The position of the base station antenna. Can also be read from the RINEX header of the base station file.

## Processing and Data Export

To start processing, press the **Start Processing** button on the bottom left.

After processing is complete, the track may be exported to Excel using **File->Export Data->Export to Excel** menu.

## SW Maps Correction
SW GNSS can be used to apply post-processed corrections to SW Maps projects. To do so, select an SW Maps project (swmz) before starting processing.

Corrected coordinates and change in position is shown for every SW Maps feature point in table form in the **SW Maps Corrections** tab.

The corrected SW Maps project can be exported as Shapefiles, KMZ, GPKG or SWMZ from the **File** menu.

## Map View
The **Map** tab shows a map with the solution of the last processing session. 

The map can be moved around by clicking the left mouse button and dragging. Double clicking the middle mouse button (mouse wheel) will zoom to extents.

Background maps may be added using the **Map->Tile Sources** menu. Tile sources are to be provided in XYZ format. For example, OpenStreetMap source may be added as https://a.tile.openstreetmap.org/${z}/${x}/${y}.png .

The solution may also be opened in RTKPLOT using **Map->Open in RTKPLOT** menu.


