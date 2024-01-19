import React from "react"
import { TextField, Autocomplete, Typography, IconButton, Snackbar } from "@mui/material"
import Bar from '../bar/Bar'
import { useState, useEffect } from "react"
import Tag from "../../models/Tag"
import Location from "../../models/Location"
import TagService from "../../services/TagService"
import LocationService from "../../services/LocationService"
import {Stack, Chip} from "@mui/material"
import AddIcon from '@mui/icons-material/Add'
import theme from "../Theme"
import { text } from "stream/consumers"
import { useAuthContext } from "../../contexts/auth.context"
import NewsService from "../../services/NewsService"
import CloseIcon from '@mui/icons-material/Close'

interface props{}

interface tagProps{
    options: Tag[]
    getOptionLabel: string[]
}

const WriteNews: React.FC<props> = (props) => {
    const { isAuthenticated, signout, user } = useAuthContext();
    const myid: number = user?.id ?? -1
    const [titleInput, setTitleInput] = useState<string>('Title')
    const [summaryInput, setSummaryInput] = useState<string>('Summary')
    const [textInput, setTextInput] = useState<string>('Text')
    const [urlInput, setUrlInput] = useState<string>('url')

    const [tags, setTags] = useState<Tag[]>([]);
    const [locations, setLocations] = useState<Location[]>([]);
    const [selectedTag, setSelectedTag] = useState<string | null>(null);
    const [chosenTag, setChosenTag] = useState<Tag | null>(null);
    const [selectedLocation, setSelectedLocation] = useState<Location | null>(null);
    const [selectedTags, setSelectedTags] = useState<Tag[]>([])     //used to create news
    const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false)



    const getTags = async (input: string | null) => {
        if (input === null){
            input = ""
        }
        return await TagService.GetTagsByName(input)
    }
    
    const getLocations = async (input: string) => {
        return await LocationService.GetLocationsByPrefix(input)
    }

    const insertListedTag = async (tag: Tag | null) => {
            if (chosenTag === null  || tag?.name === ''){
                return
            }

            const t: Tag = new Tag(tag?.id ?? -1, tag?.name ?? '')
            const arr = selectedTags.filter(x => {
                return t.id === x.id
            })
            if (arr.length !== 0){
                return
            }
            setSelectedTags([...selectedTags, t])
            console.log(selectedTags)
            setChosenTag(null)
    }

    const handleDeleteTag = async (id: number) => {
        setSelectedTags(selectedTags.filter(tag => 
            tag.id !== id
        ))
        console.log(`Tag is deleted, now selected tags are ${JSON.stringify(selectedTags)}`)
    }

    const writeNews = async () => {
        if (titleInput === '' || 
            summaryInput === '' ||
            textInput === '' || 
            selectedLocation === null){
                return
            }

        
        const tagIds = selectedTags.map(tag => tag.id)
        if (tagIds.length === 0){
            return
        }
        const jsonObject = {
            title: titleInput,
            summary: summaryInput,
            text: textInput,
            imageUrl: urlInput,
            tagsIds: tagIds,
            authorId: myid,
            locationId: selectedLocation.id
        }

        const okay = await NewsService.CreateNews(jsonObject)

        if (okay){
            setSnackbarOpen(true)
        }
    }
    
    const handleCloseSnackbar = () => {
        setSnackbarOpen(false)
    }

    const action = (
        <React.Fragment>
          <IconButton
            size="small"
            aria-label="close"
            color="inherit"
            onClick={handleCloseSnackbar}
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        </React.Fragment>
      );

    return(
        <>
            <Bar />
            <div style={{width: '80%', marginLeft: '5%' }}>
                
                <Stack direction="row">
                    <Autocomplete
                        style={{marginTop: '5%', marginBottom: '1%', width: '60%'}}
                        options={tags.map((tag) => tag.name)}
                        getOptionLabel={(option) => option}
                        value={selectedTag}
                        onChange={async (_, newValue) => {
                            const newTags = await getTags(newValue);
                            await setChosenTag(newTags[0])
                            console.log(`Chosen tag is {id:${newTags[0].id} name:${newTags[0].name}}`)
                        }}
                        onSelect={ (_) => {

                            const a = 5
                            const b = 6
                        }}
                        onInputChange={async (_, newInputValue) => {
                        const newTags = await getTags(newInputValue);
                        setTags(newTags);
                        }}
                        renderInput={(params) => (
                        <TextField {...params} label="Tag" variant="outlined" />
                        )}
                        autoComplete
                    />
                    <IconButton color="inherit" onClick={e => {
                        insertListedTag(chosenTag)
                    }}
                        style={{ height: '50%', borderRadius: '10%', backgroundColor: theme.palette.primary.main,
                         color: theme.palette.primary.contrastText}}>
                        <AddIcon />
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">ADD</Typography>
                    </IconButton>
                </Stack>
                <Stack direction="row" spacing={1} style={{marginBottom: '5%'}}>
                    {selectedTags.map(tag => {
                        return <Chip key={tag.id} label={tag.name} onDelete={e => {
                            handleDeleteTag(tag.id)}
                        } />
                    })}
                </Stack>
                <Autocomplete
                    
                    style={{marginTop: '1%', marginBottom: '1%', width: '40%'}}
                    options={locations.map((location) => location.name)}
                    getOptionLabel={(option) => option}
                    value={selectedLocation?.name ?? ""}
                    onChange={async (_, newValue) => {
                        const loc = await LocationService.GetLocationByName(newValue ?? "")
                        setSelectedLocation(loc)
                    }}
                    onInputChange={async (_, newInputValue) => {
                        const newLocations = await getLocations(newInputValue);
                        setLocations(newLocations);
                    }}
                    renderInput={(params) => (
                    <TextField {...params} label="Location" variant="outlined" />
                    )}
                />
                <div>
                <TextField
                    style={{marginTop: '1%'}}
                    required
                    id="filled-required"
                    label="Title"
                    defaultValue="Title"
                    variant="standard"
                    onChange={e => {
                        setTitleInput(e.target.value)
                    }}
                />
                </div>
                <div>
                <TextField
                    style={{marginTop: '1%'}}
                    required
                    id="filled-required"
                    label="Summary"
                    defaultValue="Summary"
                    variant="standard"
                    onChange={e => {
                        setSummaryInput(e.target.value)
                    }}
                />
                </div>
                <div>
                <TextField
                    style={{marginTop: '1%'}}
                    required
                    id="filled-required"
                    label="url"
                    defaultValue="url"
                    variant="standard"
                    onChange={e => {
                        setUrlInput(e.target.value)
                    }}
                />
                </div>
                <TextField

                    id="standard-multiline-static"
                    label="Text"
                    multiline
                    rows={8}
                    defaultValue="Text"
                    variant="standard"
                    style={{width: '80%'}}
                    onChange={e => {
                        setTextInput(e.target.value)
                    }}
                />
                <div>
                    <IconButton color="inherit" onClick={e => {
                        writeNews()
                    }}
                        style={{ height: '50%', borderRadius: '5%', backgroundColor: theme.palette.primary.main,
                         color: theme.palette.primary.contrastText, marginTop: '10%', marginBottom: '15%'}}>
                        <AddIcon />
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">WRITE NEWS</Typography>
                    </IconButton>
                </div>
            </div>

            <Snackbar
                open={snackbarOpen}
                autoHideDuration={60000}
                onClose={handleCloseSnackbar}
                message="News added"
                action={action}
            />
        </>
    )
}

export default WriteNews