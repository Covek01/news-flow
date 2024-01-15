

class News{
    title: string
    imageUrl: string
    authorName: string
    summary: string
    text: string
    authorId: number
    viewsCount: number
    likesCount: number

    constructor(title: string,
        url: string,
        authorName: string, 
        summary: string,
        text: string,
        authorId: number,
        viewsCount: number,
        likesCount: number)
    {
        this.title = title
        this.imageUrl = url
        this.authorName = authorName
        this.summary = summary
        this.text = text
        this.authorId = authorId
        this.viewsCount = viewsCount
        this.likesCount = likesCount
    }
}

export default News