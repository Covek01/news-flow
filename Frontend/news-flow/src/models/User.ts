

export default class User{
    id: number
    name: string
    email: string
    phone?: string
    country: string
    city: string
    imageUrl?: string
    role: string
    isJournalist: boolean

    constructor(id: number, name: string, email: string, phone: string, country: string, city: string, imageUrl: string, role: string){
        this.id = id
        this.name = name
        this.email = email
        this.phone = phone
        this.country = country
        this.city = city
        this.imageUrl = imageUrl
        this.role = role
        this.isJournalist = this.role === "Journalist"
    }
}
