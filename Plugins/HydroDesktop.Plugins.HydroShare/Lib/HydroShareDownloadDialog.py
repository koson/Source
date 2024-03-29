"""Subclass of MyFrame1, which is generated by wxFormBuilder."""

import wx
import GetShapefiles
import hydrosharedownload
import sys
import urllib2
import json
import time

# Implementing MyFrame1
class HydroShareDownloadDialog( GetShapefiles.MyFrame1 ):
    hydrosharedownloader = None

    def __init__( self, parent ):
        GetShapefiles.MyFrame1.__init__( self, parent )
        self.hydrosharedownloader = hydrosharedownload.HydroshareDownloader()
        self.gag_ProgressBar.Show(False)
        
    def populateList(self):
        filtered_files = self.hydrosharedownloader.retrieveList(self.cmb_FilterSearch.GetStringSelection())
        i=0
        for item in filtered_files:
            self.lst_AvailableItems.Insert(item, i)
            i+=1

    def populateFilterSearch(self):

        self.cmb_FilterSearch.Insert("All", 0)

        #Load the data from the url into data
        data = urllib2.urlopen("http://dev.hydroshare.org/?q=my_services/node.json&api-key=581d46dd")

        #Turn the data from a raw string into JSON
        all_files = json.load(data)

        resource_types = []
        usableResourceTypes = ["hydroshare_geoanalytics", "hydroshare_time_series"]

        #Look through all nodes in the JSON data
        for file in all_files:

            #Cycle through the files and extract unique resource types to add to resource_types
            if (file["type"] not in resource_types and file["type"] in usableResourceTypes):
                resource_types.append(file["type"])
        i=1
        for item in resource_types:
            self.cmb_FilterSearch.Insert(item, i)
            i+=1
    
    # Handlers for MyFrame1 events.
    def clk_FilterSearch( self, event ):
        self.lst_AvailableItems.Clear()
        self.populateList()
    
    def clk_Cancel( self, event ):
        self.Close()

    def ProgressBar(self):
        self.gag_ProgressBar.Show()
        self.Update()
        for n in range(100):
            time.sleep(0.01)
            self.gag_ProgressBar.Pulse()
            self.Update()
    
    def clk_GetData( self, event ):
        self.ProgressBar()
        if self.lst_AvailableItems.GetSelections() >= 0:
            selected_items = []
            selected_items = self.lst_AvailableItems.GetSelections()
            for item in selected_items:
                self.hydrosharedownloader.downloadFile(self.lst_AvailableItems.GetString(item))
            self.Close()
    
def open():
    app = wx.App(0)
    
    frame = HydroShareDownloadDialog(None)
    frame.Show()
    frame.populateList()
    frame.populateFilterSearch()

    #If there is an argument provided when running this, set it as our downloader's savepath.
    if len(sys.argv) > 1:
        frame.hydrosharedownloader.file_path = sys.argv[1]
    app.MainLoop()
    
if __name__ == '__main__':
    open()
    