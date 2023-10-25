import { BsPlusCircleFill, BsXCircle } from "react-icons/bs";
import { QueuedVideo } from "../types/QueuedVideo";
import Button from "./Button";
import { TextInput } from "./TextInput";
import { useEffect, useRef, useState } from "react";

export interface PlaylistProps {
  onChange?: (queuedVideos: QueuedVideo[]) => void;
}

export default function Playlist({onChange}: PlaylistProps) {
  const [queuedVideos, setQueuedVideos] = useState<QueuedVideo[]>([]);
  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");
  const queuedVideosRef = useRef<HTMLDivElement>(null);

  const videoThumbnailStyle = {
    width: "60px",
    height: "45px"
  };

  useEffect(() => {
    if (queuedVideosRef.current) {
      queuedVideosRef.current.scrollTop = queuedVideosRef.current.scrollHeight;
    }
  }, [queuedVideos]);

  const handleTextInputChange = (text: string) => {
    setCurrentVideoUrlText(text);
  }

  const handleAddVideoUrlButtonClick = () => {
    if (!currentVideoUrlText || currentVideoUrlText?.length === 0) {
      return;
    }

    const newQueuedVideo: QueuedVideo = {
      url: "https://www.youtube.com/watch?v=p2zMXSXhZ9M",
      image: "https://img.youtube.com/vi/UgkvcWx8oXw/2.jpg",
      title: currentVideoUrlText
      //title can be fetched from webpage's title (from URL)
    };

    setQueuedVideos([...queuedVideos, newQueuedVideo])
    setCurrentVideoUrlText("");
  }

  useEffect(() => {
    if (onChange) {
      onChange(queuedVideos);
    }
  }, [queuedVideos]);

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleAddVideoUrlButtonClick();
    }
  }

  const handleRemoveQueuedVideoButtonClick = (event: any, index: number) => {
    console.log(index);
    event.preventDefault();
    setQueuedVideos(queuedVideos.filter((_, i) => i !== index));
  }

  return (
    <>
      <div className="d-flex mb-3">
        <TextInput
          classNames="form-control rounded-0"
          value={currentVideoUrlText}
          placeholder="Paste video URL"
          onChange={handleTextInputChange}
          onKeyDown={handleEnterPress}
        />
        <Button
          text={<BsPlusCircleFill />}
          classNames="btn btn-primary rounded-0"
          onClick={handleAddVideoUrlButtonClick}
        />
      </div>
      <div className="list-group rounded-3 control-panel-list" ref={queuedVideosRef}>
      {
        queuedVideos.length !== 0 ? (
          queuedVideos.map((queuedVideo, index) => (
            <a 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2 a-video"
              href={queuedVideo.url}
              target={"_blank"}
            >
              <div className="row">
                <div className="col-auto">
                    {queuedVideo.image !== undefined && <img src={queuedVideo.image} style={videoThumbnailStyle} alt="Video Thumbnail" />}
                </div>
                <div className="d-flex col justify-content-between align-items-center text-secondary align-items-center">
                  <small>{queuedVideo.title !== undefined ? queuedVideo.title : queuedVideo.url}</small>
                  <div>
                    <Button
                      text={<BsXCircle/>}
                      classNames="btn btn-outline-danger btn-sm"
                      styles={{marginLeft: "5px", }}
                      onClick={() => handleRemoveQueuedVideoButtonClick(event, index)}
                    />
                  </div>
                </div>
              </div>
            </a>
          ))
        ) :
        <h6 className="text-white text-center">No videos to display</h6>
      }
      </div>
    </>
  )
}
