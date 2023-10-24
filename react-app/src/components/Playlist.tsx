import { QueuedVideo } from "../types/QueuedVideo";

export interface PlaylistProps {
  queuedVideos: QueuedVideo[];
}

export default function Playlist({queuedVideos}: PlaylistProps) {
  return (
    <ul className="list-group rounded-3">
    {
      queuedVideos.length !== 0 ? (
        queuedVideos.map((queuedVideo, index) => (
          <li 
            key={index}
            className="border border-secondary list-group-item bg-muted border-2 opacity-75"
          >
            <h5 className="d-inline">{queuedVideo.url}</h5>
            <h6 className="text-dark">{queuedVideo.image}</h6>
          </li>
        ))
      ) :
      <h6 className="text-white text-center">No videos to display</h6>
    }
    </ul>
  )
}
