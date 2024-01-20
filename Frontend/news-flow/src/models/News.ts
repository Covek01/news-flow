

class News{
    id: number
    title: string
    imageUrl: string
    authorName: string
    summary: string
    text: string
    authorId: number
    viewsCount: number
    likeCount: number
    postTime: Date

    constructor(
        id: number = 0,
        title: string = "",
        url: string = "",
        authorName: string = "", 
        summary: string = "",
        text: string = "",
        authorId: number = 0,
        viewsCount: number = 0,
        likeCount: number = 0,
        postTime: Date = new Date())
    {
        this.id = id
        this.title = title
        this.imageUrl = url
        this.authorName = authorName
        this.summary = summary
        this.text = text
        this.authorId = authorId
        this.viewsCount = viewsCount
        this.likeCount = likeCount
        this.postTime = postTime
    }
    
}

export default News