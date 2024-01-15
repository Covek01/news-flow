

export default class User{
    name: string
    email: string
    phone?: string
    country: string
    city: string
    imageUrl?: string
    role: string
    isJournalist: boolean

    constructor(name: string, email: string, phone: string, country: string, city: string, imageUrl: string, role: string){
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
