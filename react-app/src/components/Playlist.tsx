import { BsPlusCircleFill, BsXCircle } from "react-icons/bs";
import { QueuedVideo } from "../types/QueuedVideo";
import Button from "./Button";
import { InputForm } from "./InputForm";
import { useEffect, useRef, useState } from "react";
import VideoIcon from './../assets/video-icon.png'

export interface PlaylistProps {
  initialQueuedVideos: QueuedVideo[];
  onChange?: (queuedVideos: QueuedVideo[]) => void;
}

export default function Playlist({initialQueuedVideos, onChange}: PlaylistProps) {
  const [queuedVideos, setQueuedVideos] = useState<QueuedVideo[]>(initialQueuedVideos);
  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");
  const queuedVideosRef = useRef<HTMLDivElement>(null);

  const videoThumbnailStyle = {
    width: "40px",
    height: "40px"
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
      url: currentVideoUrlText,
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
    event.preventDefault();
    setQueuedVideos(queuedVideos.filter((_, i) => i !== index));
  }

  return (
    <>
      <div className="d-flex mb-3">
        <InputForm
          classNames="form-control rounded-0"
          value={currentVideoUrlText}
          trim={true}
          placeholder="Paste video URL"
          isEnabled={true}
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
              href={queuedVideo.url.startsWith('http') ? queuedVideo.url : `http://${queuedVideo.url}`}
              target={"_blank"}
            >
              <div className="row">
                <div className="col-auto">
                  <img src={VideoIcon} alt="Video Icon" style={videoThumbnailStyle} />
                </div>
                <div className="d-flex col justify-content-between align-items-center text-secondary align-items-center">
                 <small style={{wordWrap: 'break-word', maxWidth: '200px'}}>{queuedVideo.url}</small>
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
