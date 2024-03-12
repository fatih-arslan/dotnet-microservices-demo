import { Auction, PagedResult } from "@/types"

type State = {
    auctions: Auction[]
    totalCount: number
    pageCount: number
}

type Actions = {
    setData: (data: PagedResult<Auction>) => void
    setCurrentPrice: (auctionId: string, amount: number) => void 
}