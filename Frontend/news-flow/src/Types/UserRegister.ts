export default class UserRegister{
    name: string
    email: string
    phone?: string
    country: string
    city: string
    role: string

    constructor(name: string, email: string, phone: string, country: string, city: string, role: string){
        this.name = name
        this.email = email
        this.phone = phone
        this.country = country
        this.city = city
        this.role = role
    }
}
