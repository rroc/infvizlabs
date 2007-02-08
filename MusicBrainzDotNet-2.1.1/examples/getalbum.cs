/* --------------------------------------------------------------------------

   Copyright (C) 2004 Sean Cier
   Copyright (C) 2000 Robert Kaye

   This library is free software; you can redistribute it and/or
   modify it under the terms of the GNU Lesser General Public
   License as published by the Free Software Foundation; either
   version 2.1 of the License, or (at your option) any later version.

   This library is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
   Lesser General Public License for more details.

   You should have received a copy of the GNU Lesser General Public
   License along with this library; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

----------------------------------------------------------------------------*/
using System;
using musicbrainz;

public class GetAlbum {

public static int Main(String[] argv) {
    MusicBrainz o;
    String      error, data, temp;
    bool        ret, isMultipleArtist = false;
    int         numTracks, trackNum, i;

    if (argv.Length != 1)
    {
        Console.WriteLine("Usage: GetAlbum <albumid|cdindexid>");
        return 0;
    }

    // Create the musicbrainz object, which will be needed for subsequent calls
    o = new MusicBrainz();

    // Set the proper server to use. Defaults to mm.musicbrainz.org:80
    if (Environment.GetEnvironmentVariable("MB_SERVER") != null)
        o.SetServer(Environment.GetEnvironmentVariable("MB_SERVER"), 80);

    // Check to see if the debug env var has been set 
    if (Environment.GetEnvironmentVariable("MB_DEBUG") != null)
        o.SetDebug(Environment.GetEnvironmentVariable("MB_DEBUG") != "0");

    // Tell the server to only return 2 levels of data, unless the MB_DEPTH env var is set
    if (Environment.GetEnvironmentVariable("MB_DEPTH") != null)
        o.SetDepth(int.Parse(Environment.GetEnvironmentVariable("MB_DEPTH")));
    else
        o.SetDepth(4);

    // Set up the args for the find album query
    String[] args = new String[] { argv[0] };

    if (argv[0].Length != MusicBrainz.CDINDEX_ID_LEN)
        // Execute the MBQ_GetAlbumById query
        ret = o.Query(MusicBrainz.MBQ_GetAlbumById, args);
    else
        // Execute the MBQ_GetCDInfoFromCDIndexId
        ret = o.Query(MusicBrainz.MBQ_GetCDInfoFromCDIndexId, args);

    if (!ret)
    {
        o.GetQueryError(out error);
        Console.WriteLine("Query failed: {0}", error);
        return 0;
    }

    // Select the first album
    o.Select(MusicBrainz.MBS_SelectAlbum, 1);  

    // Pull back the album id to see if we got the album
    if (!o.GetResultData(MusicBrainz.MBE_AlbumGetAlbumId, out data))
    {
        Console.WriteLine("Album not found.");
        return 0;
    }  
    o.GetIDFromURL(data, out temp);
    Console.WriteLine("    AlbumId: {0}", temp);

    // Extract the album name
    if (o.GetResultData(MusicBrainz.MBE_AlbumGetAlbumName, out data))
       Console.WriteLine("       Name: {0}", data);

    // Extract the number of tracks
    numTracks = o.GetResultInt(MusicBrainz.MBE_AlbumGetNumTracks);
    if (numTracks > 0 && numTracks < 100)
       Console.WriteLine("  NumTracks: {0}", numTracks);

    // Check to see if there is more than one artist for this album
    for(i = 1; i <= numTracks; i++)
    {
        if (!o.GetResultData(MusicBrainz.MBE_AlbumGetArtistId, i, out data))
            break;

        if (i == 1)
           temp = data;

        if (temp != data)
        {
           isMultipleArtist = true;
           break;
        }
    }

    if (!isMultipleArtist)
    {
        // Extract the artist name from the album
        if (o.GetResultData(MusicBrainz.MBE_AlbumGetArtistName, 1, out data))
           Console.WriteLine("AlbumArtist: {0}", data);
    
        // Extract the artist id from the album
        if (o.GetResultData(MusicBrainz.MBE_AlbumGetArtistId, 1, out data))
        {
           o.GetIDFromURL(data, out temp);
           Console.WriteLine("   ArtistId: {0}", temp);
        }
    }

    Console.WriteLine();

    for(i = 1; i <= numTracks; i++)
    {
        // Extract the track name from the album.
        if (o.GetResultData(MusicBrainz.MBE_AlbumGetTrackName, i, out data))
           Console.WriteLine("      Track: {0}", data);
        else
           break;
    
        // Extract the album id from the track. Just use the
        // first album that this track appears on
        if (o.GetResultData(MusicBrainz.MBE_AlbumGetTrackId, i, out data))
        {
           o.GetIDFromURL(data, out temp);
           Console.WriteLine("    TrackId: {0}", temp);

           // Extract the track number
           trackNum = o.GetOrdinalFromList(MusicBrainz.MBE_AlbumGetTrackList, data);
           if (trackNum > 0 && trackNum < 100)
               Console.WriteLine("  TrackNum: {0}", trackNum);
        }

        // If its a multple artist album, print out the artist for each track
        if (isMultipleArtist)
        {
           // Extract the artist name from this track
           if (o.GetResultData(MusicBrainz.MBE_AlbumGetArtistName, i, out data))
              Console.WriteLine("TrackArtist: {0}", data);
       
           // Extract the artist id from this track
           if (o.GetResultData(MusicBrainz.MBE_AlbumGetArtistId, i, out data))
           {
              o.GetIDFromURL(data, out temp);
              Console.WriteLine("   ArtistId: {0}", temp);
           }
        }
        Console.WriteLine();
    }

    return 0;
}

}
