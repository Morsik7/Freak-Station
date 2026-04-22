using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SponsorManager
{
    public static int GetDonateLevel(string uid)
    {
        int result = 0;
        if (SponsorInfoComponent.listOfSponsors.FirstOrDefault(p => p.Uid == uid) is { DonateLevel: var donatelevel })
        {
            result = donatelevel;
            return result;
        }
        return result;
    }
}
